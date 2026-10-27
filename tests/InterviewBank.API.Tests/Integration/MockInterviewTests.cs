using System.Net;
using System.Net.Http.Json;
using InterviewBank.API.Tests.Infrastructure;

namespace InterviewBank.API.Tests.Integration;

[Collection(ApiCollection.Name)]
public class MockInterviewTests(ApiFactory factory)
{
    private const int GotIt = 1, Partial = 2, Missed = 3;

    [Fact]
    public async Task Starting_without_questions_returns_a_helpful_error()
    {
        var (client, _) = await factory.RegisterAsync();

        var res = await client.PostAsJsonAsync("/api/mock-interview/start",
            new { questionCount = 5, strategy = 0, timePerQuestion = 60 });

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Start_rejects_invalid_settings()
    {
        var (client, _) = await factory.RegisterAsync();

        var res = await client.PostAsJsonAsync("/api/mock-interview/start",
            new { questionCount = 5, strategy = 0, timePerQuestion = 5 });

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Session_includes_model_answers_and_yes_no_correct_answer()
    {
        var (client, _) = await factory.RegisterAsync();
        var topic = await client.SystemTopicAsync("JavaScript");
        await client.CreateQuestionAsync(topic.Id, "What is a closure?", expectedAnswer: "A function plus its scope.");
        await client.CreateQuestionAsync(topic.Id, "Is NaN === NaN?", questionType: 1, expectedAnswer: "no");

        var session = await client.StartSessionAsync();

        Assert.Equal(2, session.Questions.Count);
        var essay = session.Questions.Single(q => q.QuestionType == 0);
        Assert.Equal("A function plus its scope.", essay.ExpectedAnswer);
        Assert.Null(essay.CorrectAnswer);
        Assert.Equal("No", session.Questions.Single(q => q.QuestionType == 1).CorrectAnswer);
    }

    [Fact]
    public async Task Completing_a_session_saves_results_and_updates_spaced_repetition()
    {
        var (client, _) = await factory.RegisterAsync();
        var topic = await client.SystemTopicAsync("SQL");
        var q1 = await client.CreateQuestionAsync(topic.Id, "What is a JOIN?");
        var q2 = await client.CreateQuestionAsync(topic.Id, "What is normalisation?");
        var q3 = await client.CreateQuestionAsync(topic.Id, "What is a CTE?");

        var session = await client.StartSessionAsync();
        var res = await client.CompleteSessionAsync(session.SessionId, (q1.Id, GotIt), (q2.Id, Partial), (q3.Id, Missed));

        await res.EnsureStatusAsync(HttpStatusCode.OK);
        var summary = await res.Content.ReadFromJsonAsync<SessionSummary>();
        Assert.Equal((3, 1, 1, 1), (summary!.TotalQuestions, summary.GotItCount, summary.PartialCount, summary.MissedCount));
        Assert.Equal(20, summary.AverageTimeTaken);
        Assert.All(summary.Results, r => Assert.Equal("my answer", r.UserAnswer));

        var gotIt = await client.GetFromJsonAsync<QuestionResponse>($"/api/questions/{q1.Id}");
        Assert.True(gotIt!.IsPracticed);
        Assert.Equal(1, gotIt.PracticeCount);

        // Only the missed question is due again right away
        var due = await client.GetFromJsonAsync<DueCount>("/api/mock-interview/due-count");
        Assert.Equal(1, due!.Count);

        var review = await client.StartSessionAsync(strategy: 3);
        Assert.Equal(q3.Id, Assert.Single(review.Questions).Id);
    }

    [Fact]
    public async Task Session_can_only_be_completed_once_by_its_owner()
    {
        var (alice, _) = await factory.RegisterAsync();
        var (bob, _)   = await factory.RegisterAsync();
        var topic = await alice.SystemTopicAsync("C#");
        var q = await alice.CreateQuestionAsync(topic.Id);
        var session = await alice.StartSessionAsync();

        var stolen = await bob.CompleteSessionAsync(session.SessionId, (q.Id, GotIt));
        Assert.Equal(HttpStatusCode.BadRequest, stolen.StatusCode);

        await (await alice.CompleteSessionAsync(session.SessionId, (q.Id, GotIt))).EnsureStatusAsync(HttpStatusCode.OK);

        var twice = await alice.CompleteSessionAsync(session.SessionId, (q.Id, GotIt));
        Assert.Equal(HttpStatusCode.BadRequest, twice.StatusCode);
    }

    [Fact]
    public async Task Answers_for_questions_outside_the_session_are_ignored()
    {
        var (alice, _) = await factory.RegisterAsync();
        var (bob, _)   = await factory.RegisterAsync();
        var aliceTopic = await alice.SystemTopicAsync("C#");
        var aliceQ = await alice.CreateQuestionAsync(aliceTopic.Id);
        var bobQ   = await bob.CreateQuestionAsync(aliceTopic.Id);
        var session = await alice.StartSessionAsync();

        var res = await alice.CompleteSessionAsync(session.SessionId, (aliceQ.Id, GotIt), (bobQ.Id, GotIt));

        await res.EnsureStatusAsync(HttpStatusCode.OK);
        Assert.Equal(1, (await res.Content.ReadFromJsonAsync<SessionSummary>())!.TotalQuestions);
        var untouched = await bob.GetFromJsonAsync<QuestionResponse>($"/api/questions/{bobQ.Id}");
        Assert.Equal(0, untouched!.PracticeCount);
    }

    [Fact]
    public async Task Ending_early_only_counts_answered_questions()
    {
        var (client, _) = await factory.RegisterAsync();
        var topic = await client.SystemTopicAsync("Algorithms");
        var q1 = await client.CreateQuestionAsync(topic.Id, "What is binary search?");
        await client.CreateQuestionAsync(topic.Id, "What is quicksort?");

        var session = await client.StartSessionAsync();
        var res = await client.CompleteSessionAsync(session.SessionId, (q1.Id, GotIt));

        Assert.Equal(1, (await res.Content.ReadFromJsonAsync<SessionSummary>())!.TotalQuestions);
    }

    [Fact]
    public async Task History_lists_only_completed_sessions()
    {
        var (client, _) = await factory.RegisterAsync();
        var topic = await client.SystemTopicAsync("SQL");
        var q = await client.CreateQuestionAsync(topic.Id);

        var finished = await client.StartSessionAsync();
        await client.CompleteSessionAsync(finished.SessionId, (q.Id, Partial));
        await client.StartSessionAsync(); // abandoned

        var history = await client.GetFromJsonAsync<List<PastSession>>("/api/mock-interview/sessions");

        Assert.Equal(finished.SessionId, Assert.Single(history!).SessionId);
    }

    [Fact]
    public async Task Question_with_session_history_can_still_be_deleted()
    {
        var (client, _) = await factory.RegisterAsync();
        var topic = await client.SystemTopicAsync("SQL");
        var q = await client.CreateQuestionAsync(topic.Id);
        var session = await client.StartSessionAsync();
        await client.CompleteSessionAsync(session.SessionId, (q.Id, GotIt));

        var res = await client.DeleteAsync($"/api/questions/{q.Id}");

        await res.EnsureStatusAsync(HttpStatusCode.NoContent);
    }

    private record DueCount(int Count);
}
