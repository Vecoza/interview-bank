using System.Net;
using System.Net.Http.Json;
using InterviewBank.API.Tests.Infrastructure;

namespace InterviewBank.API.Tests.Integration;

[Collection(ApiCollection.Name)]
public class DashboardAndAiTests(ApiFactory factory)
{
    private record Breakdown(int Total, int Practiced);
    private record TopicBreakdown(string TopicName, int Total, int Practiced);
    private record WeakTopic(string TopicName, int MissedCount, int TotalAnswered, double MissedRatio);
    private record Stats(int TotalQuestions, int PracticedCount, Dictionary<string, Breakdown> ByDifficulty,
        List<TopicBreakdown> ByTopic, List<PastSession> RecentSessions, List<WeakTopic> WeakestTopics,
        int PracticeStreak, int DueForReviewCount);
    private record Evaluation(int Score, string Feedback);

    [Fact]
    public async Task Dashboard_is_empty_for_a_new_user()
    {
        var (client, _) = await factory.RegisterAsync();

        var stats = await client.GetFromJsonAsync<Stats>("/api/dashboard/stats");

        Assert.Equal(0, stats!.TotalQuestions);
        Assert.Equal(0, stats.PracticeStreak);
        Assert.Empty(stats.RecentSessions);
    }

    [Fact]
    public async Task Dashboard_reflects_practice_sessions()
    {
        var (client, _) = await factory.RegisterAsync();
        var sql = await client.SystemTopicAsync("SQL");
        var js  = await client.SystemTopicAsync("JavaScript");
        var sqlQs = new[]
        {
            await client.CreateQuestionAsync(sql.Id, "SQL 1", difficulty: 1),
            await client.CreateQuestionAsync(sql.Id, "SQL 2", difficulty: 2),
            await client.CreateQuestionAsync(sql.Id, "SQL 3", difficulty: 3)
        };
        await client.CreateQuestionAsync(js.Id, "JS 1", difficulty: 1);

        var session = await client.StartSessionAsync(count: 10);
        await client.CompleteSessionAsync(session.SessionId,
            (sqlQs[0].Id, 3), (sqlQs[1].Id, 3), (sqlQs[2].Id, 1));

        var stats = await client.GetFromJsonAsync<Stats>("/api/dashboard/stats");

        Assert.Equal(4, stats!.TotalQuestions);
        Assert.Equal(3, stats.PracticedCount);
        Assert.Equal(new Breakdown(2, 1), stats.ByDifficulty["Easy"]);
        Assert.Equal(new TopicBreakdown("SQL", 3, 3), stats.ByTopic.First());
        Assert.Equal(1, stats.PracticeStreak);
        Assert.Equal(3, stats.DueForReviewCount); // JS never practiced + two missed SQL
        var recent = Assert.Single(stats.RecentSessions);
        Assert.Equal((1, 2), (recent.GotItCount, recent.MissedCount));
        var weak = Assert.Single(stats.WeakestTopics);
        Assert.Equal("SQL", weak.TopicName);
        Assert.Equal(2.0 / 3, weak.MissedRatio, 3);
    }

    [Fact]
    public async Task Ai_evaluation_returns_score_and_feedback()
    {
        var (client, _) = await factory.RegisterAsync();

        var res = await client.PostAsJsonAsync("/api/ai/evaluate-answer", new
        {
            questionText = "What is a closure?", expectedAnswer = "A function with its scope.", userAnswer = "A function."
        });

        await res.EnsureStatusAsync(HttpStatusCode.OK);
        Assert.Equal(new Evaluation(80, "Good answer, but mention the edge cases."),
            await res.Content.ReadFromJsonAsync<Evaluation>());
        Assert.Equal("A function.", factory.Evaluator.LastCall!.Value.Answer);
    }

    [Fact]
    public async Task Ai_evaluation_validates_input_and_reports_outages()
    {
        var (client, _) = await factory.RegisterAsync();
        var valid = new { questionText = "Q", expectedAnswer = "A", userAnswer = "B" };

        var blank = await client.PostAsJsonAsync("/api/ai/evaluate-answer",
            new { questionText = "Q", expectedAnswer = "A", userAnswer = "" });
        Assert.Equal(HttpStatusCode.BadRequest, blank.StatusCode);

        try
        {
            factory.Evaluator.Fail = true;
            Assert.Equal(HttpStatusCode.BadGateway,
                (await client.PostAsJsonAsync("/api/ai/evaluate-answer", valid)).StatusCode);

            factory.Evaluator.IsConfigured = false;
            Assert.Equal(HttpStatusCode.ServiceUnavailable,
                (await client.PostAsJsonAsync("/api/ai/evaluate-answer", valid)).StatusCode);
        }
        finally
        {
            factory.Evaluator.Fail = false;
            factory.Evaluator.IsConfigured = true;
        }
    }

    [Fact]
    public async Task Ai_evaluation_requires_login()
    {
        var res = await factory.CreateApiClient().PostAsJsonAsync("/api/ai/evaluate-answer",
            new { questionText = "Q", expectedAnswer = "A", userAnswer = "B" });

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }
}
