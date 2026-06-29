using System.Net.Http.Json;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterviewBank.API.Controllers;

public class EvaluateAnswerDto
{
    [Required] public string QuestionText   { get; set; } = string.Empty;
    [Required] public string ExpectedAnswer { get; set; } = string.Empty;
    [Required] public string UserAnswer     { get; set; } = string.Empty;
}

[ApiController]
[Route("api/ai")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration    _config;

    public AiController(IHttpClientFactory http, IConfiguration config)
    {
        _http   = http;
        _config = config;
    }

    [HttpPost("evaluate-answer")]
    public async Task<IActionResult> EvaluateAnswer([FromBody] EvaluateAnswerDto dto)
    {
        var apiKey = _config["Anthropic:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
            return StatusCode(503, new { error = "AI evaluation is not configured. Add Anthropic:ApiKey to appsettings." });

        var prompt = $$"""
            You are a technical interview evaluator. Score the candidate's answer strictly and fairly.

            Question: {{dto.QuestionText}}

            Model answer: {{dto.ExpectedAnswer}}

            Candidate's answer: {{dto.UserAnswer}}

            Reply with ONLY valid JSON in this exact shape (no prose, no markdown):
            {"score":<integer 0-100>,"feedback":"<one or two sentences>"}

            Scoring guide: 90-100 perfect, 70-89 good, 50-69 partial, 0-49 mostly wrong.
            If the candidate's answer is blank or off-topic, score 0.
            """;

        var body = new
        {
            model      = "claude-haiku-4-5-20251001",
            max_tokens = 256,
            messages   = new[] { new { role = "user", content = prompt } }
        };

        var client = _http.CreateClient();
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        HttpResponseMessage response;
        try
        {
            response = await client.PostAsJsonAsync("https://api.anthropic.com/v1/messages", body);
        }
        catch
        {
            return StatusCode(502, new { error = "Could not reach the AI service." });
        }

        if (!response.IsSuccessStatusCode)
            return StatusCode(502, new { error = "AI service returned an error." });

        var raw  = await response.Content.ReadAsStringAsync();
        using var doc  = JsonDocument.Parse(raw);
        var text = doc.RootElement.GetProperty("content")[0].GetProperty("text").GetString() ?? "{}";

        using var resultDoc = JsonDocument.Parse(text);
        var score    = resultDoc.RootElement.GetProperty("score").GetInt32();
        var feedback = resultDoc.RootElement.GetProperty("feedback").GetString() ?? "";

        return Ok(new { score, feedback });
    }
}
