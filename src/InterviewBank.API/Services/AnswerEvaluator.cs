using System.Net.Http.Json;
using System.Text.Json;

namespace InterviewBank.API.Services;

public record AnswerEvaluation(int Score, string Feedback);

public class AnswerEvaluatorUnavailableException(string message) : Exception(message);

public interface IAnswerEvaluator
{
    bool IsConfigured { get; }

    Task<AnswerEvaluation> EvaluateAsync(string question, string expectedAnswer, string userAnswer,
        CancellationToken ct = default);
}

/// <summary>Grades a free-text answer against the model answer using the Anthropic Messages API.</summary>
public class AnthropicAnswerEvaluator : IAnswerEvaluator
{
    private const string DefaultModel = "claude-haiku-4-5";

    private readonly HttpClient     _http;
    private readonly IConfiguration _config;
    private readonly ILogger<AnthropicAnswerEvaluator> _logger;

    public AnthropicAnswerEvaluator(HttpClient http, IConfiguration config, ILogger<AnthropicAnswerEvaluator> logger)
    {
        _http   = http;
        _config = config;
        _logger = logger;
    }

    private string? ApiKey => _config["Anthropic:ApiKey"];

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ApiKey) && !ApiKey.StartsWith("YOUR_", StringComparison.Ordinal);

    public async Task<AnswerEvaluation> EvaluateAsync(string question, string expectedAnswer, string userAnswer,
        CancellationToken ct = default)
    {
        var prompt = $$"""
            You are a technical interview evaluator. Score the candidate's answer strictly and fairly.

            Question: {{question}}

            Model answer: {{expectedAnswer}}

            Candidate's answer: {{userAnswer}}

            Reply with ONLY valid JSON in this exact shape (no prose, no markdown):
            {"score":<integer 0-100>,"feedback":"<one or two sentences>"}

            Scoring guide: 90-100 perfect, 70-89 good, 50-69 partial, 0-49 mostly wrong.
            If the candidate's answer is blank or off-topic, score 0.
            """;

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        request.Headers.Add("x-api-key", ApiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = JsonContent.Create(new
        {
            model      = _config["Anthropic:Model"] ?? DefaultModel,
            max_tokens = 512,
            messages   = new[] { new { role = "user", content = prompt } }
        });

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "AI evaluation request failed");
            throw new AnswerEvaluatorUnavailableException("Could not reach the AI service.");
        }

        using (response)
        {
            var raw = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("AI service returned {Status}: {Body}", (int)response.StatusCode, raw);
                throw new AnswerEvaluatorUnavailableException("AI service returned an error.");
            }

            using var doc = JsonDocument.Parse(raw);
            var text = string.Concat(doc.RootElement.GetProperty("content").EnumerateArray()
                .Where(b => b.GetProperty("type").GetString() == "text")
                .Select(b => b.GetProperty("text").GetString()));

            return Parse(text)
                ?? throw new AnswerEvaluatorUnavailableException("AI service returned an unexpected response.");
        }
    }

    /// <summary>Extracts the score JSON even when the model wraps it in markdown or extra prose.</summary>
    public static AnswerEvaluation? Parse(string text)
    {
        var start = text.IndexOf('{');
        var end   = text.LastIndexOf('}');
        if (start < 0 || end <= start) return null;

        try
        {
            using var doc = JsonDocument.Parse(text[start..(end + 1)]);
            var root = doc.RootElement;

            if (!root.TryGetProperty("score", out var scoreEl)) return null;
            var score = scoreEl.ValueKind == JsonValueKind.Number
                ? (int)Math.Round(scoreEl.GetDouble())
                : int.TryParse(scoreEl.GetString(), out var s) ? s : -1;
            if (score < 0) return null;

            var feedback = root.TryGetProperty("feedback", out var fb) ? fb.GetString() ?? "" : "";
            return new AnswerEvaluation(Math.Clamp(score, 0, 100), feedback.Trim());
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
