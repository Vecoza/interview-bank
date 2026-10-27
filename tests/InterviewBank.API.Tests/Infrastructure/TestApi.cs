using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace InterviewBank.API.Tests.Infrastructure;

public record AuthResponse(string AccessToken, string Email, string UserId);
public record TopicResponse(Guid Id, string Name, bool IsSystem, int QuestionCount);
public record QuestionResponse(
    Guid Id, Guid TopicId, string TopicName, string Text, int Difficulty, int QuestionType,
    string? ExpectedAnswer, string? PersonalNotes, string? Source, bool IsPracticed, int PracticeCount,
    DateTimeOffset? LastPracticedAt);
public record QuestionPage(int Total, int Page, int PageSize, List<QuestionResponse> Questions);
public record LibraryItem(Guid Id, string TopicName, int Difficulty, string Text, string? ExpectedAnswer, bool AlreadyImported);
public record ImportResult(int Imported, int AlreadyInBank, List<string> UnmatchedTopics);
public record SessionQuestion(Guid Id, string Text, string TopicName, int Difficulty, int QuestionOrder,
    int QuestionType, string? CorrectAnswer, string? ExpectedAnswer);
public record ActiveSession(Guid SessionId, int TimePerQuestion, List<SessionQuestion> Questions);
public record SessionResult(string QuestionText, string? UserAnswer, string? ExpectedAnswer, int TimeTakenSeconds,
    int SelfAssessment, int QuestionOrder);
public record SessionSummary(Guid SessionId, int TotalQuestions, int GotItCount, int PartialCount, int MissedCount,
    double AverageTimeTaken, List<SessionResult> Results);
public record PastSession(Guid SessionId, DateTimeOffset CompletedAt, int TotalQuestions, int GotItCount,
    int PartialCount, int MissedCount);

public static class TestApi
{
    public const string Password = "Passw0rd!";

    public static string NewEmail() => $"user-{Guid.NewGuid():N}@example.com";

    /// <summary>Registers a fresh user and returns a client that sends their bearer token.</summary>
    public static async Task<(HttpClient Client, AuthResponse Auth)> RegisterAsync(this ApiFactory factory)
    {
        var client = factory.CreateApiClient();
        var res = await client.PostAsJsonAsync("/api/auth/register", new { email = NewEmail(), password = Password });
        await res.EnsureStatusAsync(HttpStatusCode.OK);

        var auth = (await res.Content.ReadFromJsonAsync<AuthResponse>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return (client, auth);
    }

    public static async Task<TopicResponse> SystemTopicAsync(this HttpClient client, string name)
    {
        var topics = await client.GetFromJsonAsync<List<TopicResponse>>("/api/topics");
        return topics!.Single(t => t.IsSystem && t.Name == name);
    }

    public static async Task<QuestionResponse> CreateQuestionAsync(this HttpClient client, Guid topicId,
        string text = "What is dependency injection?", int difficulty = 2, int questionType = 0,
        string? expectedAnswer = "Supplying a class's dependencies from the outside.")
    {
        var res = await client.PostAsJsonAsync("/api/questions", new
        {
            text, topicId, difficulty, questionType, expectedAnswer
        });
        await res.EnsureStatusAsync(HttpStatusCode.Created);
        return (await res.Content.ReadFromJsonAsync<QuestionResponse>())!;
    }

    public static async Task<ActiveSession> StartSessionAsync(this HttpClient client, int count = 10,
        int strategy = 0, int timePerQuestion = 60)
    {
        var res = await client.PostAsJsonAsync("/api/mock-interview/start", new
        {
            questionCount = count, strategy, timePerQuestion
        });
        await res.EnsureStatusAsync(HttpStatusCode.OK);
        return (await res.Content.ReadFromJsonAsync<ActiveSession>())!;
    }

    public static async Task<HttpResponseMessage> CompleteSessionAsync(this HttpClient client, Guid sessionId,
        params (Guid QuestionId, int Assessment)[] answers) =>
        await client.PostAsJsonAsync("/api/mock-interview/complete", new
        {
            sessionId,
            answers = answers.Select(a => new
            {
                sessionId,
                questionId       = a.QuestionId,
                userAnswer       = "my answer",
                timeTakenSeconds = 20,
                selfAssessment   = a.Assessment
            })
        });

    /// <summary>Asserts the status code and includes the response body in the failure message.</summary>
    public static async Task EnsureStatusAsync(this HttpResponseMessage res, HttpStatusCode expected)
    {
        if (res.StatusCode != expected)
        {
            var body = await res.Content.ReadAsStringAsync();
            Assert.Fail($"Expected {(int)expected} {expected} but got {(int)res.StatusCode} {res.StatusCode}: {body}");
        }
    }
}
