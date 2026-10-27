using System.Net;
using System.Net.Http.Json;
using InterviewBank.API.Tests.Infrastructure;

namespace InterviewBank.API.Tests.Integration;

[Collection(ApiCollection.Name)]
public class QuestionsAndTopicsTests(ApiFactory factory)
{
    [Fact]
    public async Task System_topics_are_seeded()
    {
        var (client, _) = await factory.RegisterAsync();

        var topics = await client.GetFromJsonAsync<List<TopicResponse>>("/api/topics");

        Assert.Equal(13, topics!.Count(t => t.IsSystem));
        Assert.Contains(topics!, t => t.Name == "C#");
    }

    [Fact]
    public async Task Question_crud_round_trip()
    {
        var (client, _) = await factory.RegisterAsync();
        var topic = await client.SystemTopicAsync("SQL");

        var created = await client.CreateQuestionAsync(topic.Id, "What is an index?", difficulty: 1);
        Assert.Equal("SQL", created.TopicName);

        var fetched = await client.GetFromJsonAsync<QuestionResponse>($"/api/questions/{created.Id}");
        Assert.Equal("What is an index?", fetched!.Text);

        var update = await client.PutAsJsonAsync($"/api/questions/{created.Id}", new
        {
            text = "What is a covering index?", topicId = topic.Id, difficulty = 3, questionType = 0,
            expectedAnswer = "An index that contains every column the query needs.", personalNotes = "Review"
        });
        await update.EnsureStatusAsync(HttpStatusCode.OK);
        var updated = await update.Content.ReadFromJsonAsync<QuestionResponse>();
        Assert.Equal(3, updated!.Difficulty);
        Assert.Equal("Review", updated.PersonalNotes);

        var delete = await client.DeleteAsync($"/api/questions/{created.Id}");
        await delete.EnsureStatusAsync(HttpStatusCode.NoContent);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/questions/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task Questions_are_private_to_their_owner()
    {
        var (alice, _) = await factory.RegisterAsync();
        var (bob, _)   = await factory.RegisterAsync();
        var topic = await alice.SystemTopicAsync("C#");
        var question = await alice.CreateQuestionAsync(topic.Id);

        Assert.Equal(HttpStatusCode.NotFound, (await bob.GetAsync($"/api/questions/{question.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await bob.DeleteAsync($"/api/questions/{question.Id}")).StatusCode);

        var bobsList = await bob.GetFromJsonAsync<QuestionPage>("/api/questions");
        Assert.Equal(0, bobsList!.Total);
    }

    [Fact]
    public async Task List_supports_search_difficulty_and_practiced_filters()
    {
        var (client, _) = await factory.RegisterAsync();
        var topic = await client.SystemTopicAsync("JavaScript");
        await client.CreateQuestionAsync(topic.Id, "Explain closures", difficulty: 1);
        var hoisting = await client.CreateQuestionAsync(topic.Id, "Explain hoisting", difficulty: 3);
        await client.CreateQuestionAsync(topic.Id, "What is the event loop?", difficulty: 3);
        await client.PatchAsync($"/api/questions/{hoisting.Id}/practiced", null);

        var search = await client.GetFromJsonAsync<QuestionPage>("/api/questions?search=EXPLAIN");
        Assert.Equal(2, search!.Total);

        var hard = await client.GetFromJsonAsync<QuestionPage>("/api/questions?difficulties=3");
        Assert.Equal(2, hard!.Total);

        var practiced = await client.GetFromJsonAsync<QuestionPage>("/api/questions?isPracticed=true");
        Assert.Equal("Explain hoisting", Assert.Single(practiced!.Questions).Text);

        var paged = await client.GetFromJsonAsync<QuestionPage>("/api/questions?page=0&pageSize=1000");
        Assert.Equal(1, paged!.Page);
        Assert.Equal(100, paged.PageSize);
    }

    [Fact]
    public async Task Yes_no_answers_are_normalised_and_validated()
    {
        var (client, _) = await factory.RegisterAsync();
        var topic = await client.SystemTopicAsync("JavaScript");

        var q = await client.CreateQuestionAsync(topic.Id, "Is null == undefined?", questionType: 1, expectedAnswer: " true ");
        Assert.Equal("Yes", q.ExpectedAnswer);

        var invalid = await client.PostAsJsonAsync("/api/questions", new
        {
            text = "Is JS typed?", topicId = topic.Id, difficulty = 1, questionType = 1, expectedAnswer = "maybe"
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
    }

    [Fact]
    public async Task Custom_topics_are_private_unique_and_protected_while_in_use()
    {
        var (alice, _) = await factory.RegisterAsync();
        var (bob, _)   = await factory.RegisterAsync();

        var create = await alice.PostAsJsonAsync("/api/topics", new { name = "Kotlin" });
        await create.EnsureStatusAsync(HttpStatusCode.Created);
        var topic = (await create.Content.ReadFromJsonAsync<TopicResponse>())!;

        // Same name for the same user conflicts, also against system topics
        Assert.Equal(HttpStatusCode.Conflict, (await alice.PostAsJsonAsync("/api/topics", new { name = "kotlin" })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await alice.PostAsJsonAsync("/api/topics", new { name = "SQL" })).StatusCode);

        // Another user doesn't see it and can create their own
        var bobsTopics = await bob.GetFromJsonAsync<List<TopicResponse>>("/api/topics");
        Assert.DoesNotContain(bobsTopics!, t => t.Id == topic.Id);
        await (await bob.PostAsJsonAsync("/api/topics", new { name = "Kotlin" })).EnsureStatusAsync(HttpStatusCode.Created);

        // ...and can't put questions in Alice's topic
        var foreign = await bob.PostAsJsonAsync("/api/questions", new { text = "Q", topicId = topic.Id, difficulty = 1 });
        Assert.Equal(HttpStatusCode.BadRequest, foreign.StatusCode);

        var question = await alice.CreateQuestionAsync(topic.Id);
        Assert.Equal(HttpStatusCode.Conflict, (await alice.DeleteAsync($"/api/topics/{topic.Id}")).StatusCode);

        await alice.DeleteAsync($"/api/questions/{question.Id}");
        await (await alice.DeleteAsync($"/api/topics/{topic.Id}")).EnsureStatusAsync(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task System_topics_cannot_be_deleted()
    {
        var (client, _) = await factory.RegisterAsync();
        var topic = await client.SystemTopicAsync("General");

        var res = await client.DeleteAsync($"/api/topics/{topic.Id}");

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }
}
