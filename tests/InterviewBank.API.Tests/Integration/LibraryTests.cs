using System.Net;
using System.Net.Http.Json;
using InterviewBank.API.Tests.Infrastructure;

namespace InterviewBank.API.Tests.Integration;

[Collection(ApiCollection.Name)]
public class LibraryTests(ApiFactory factory)
{
    [Fact]
    public async Task Library_is_seeded_with_four_topics()
    {
        var (client, _) = await factory.RegisterAsync();

        var items = await client.GetFromJsonAsync<List<LibraryItem>>("/api/library");

        Assert.Equal(51, items!.Count);
        Assert.Equal(["Algorithms", "C#", "JavaScript", "SQL"], items.Select(i => i.TopicName).Distinct().Order());
        Assert.All(items, i => Assert.False(string.IsNullOrWhiteSpace(i.ExpectedAnswer)));
    }

    [Fact]
    public async Task Library_can_be_filtered()
    {
        var (client, _) = await factory.RegisterAsync();

        var items = await client.GetFromJsonAsync<List<LibraryItem>>("/api/library?topicName=SQL&difficulty=1");

        Assert.NotEmpty(items!);
        Assert.All(items!, i => Assert.True(i.TopicName == "SQL" && i.Difficulty == 1));
    }

    [Fact]
    public async Task Import_copies_questions_once_and_marks_them_as_added()
    {
        var (client, _) = await factory.RegisterAsync();
        var library = await client.GetFromJsonAsync<List<LibraryItem>>("/api/library?topicName=C%23");
        var ids = library!.Take(3).Select(i => i.Id).ToList();

        var first = await client.PostAsJsonAsync("/api/library/import", new { ids });
        await first.EnsureStatusAsync(HttpStatusCode.OK);
        var firstResult = await first.Content.ReadFromJsonAsync<ImportResult>();
        Assert.Equal((3, 0), (firstResult!.Imported, firstResult.AlreadyInBank));
        Assert.Empty(firstResult.UnmatchedTopics);

        var again = await client.PostAsJsonAsync("/api/library/import", new { ids });
        var againResult = await again.Content.ReadFromJsonAsync<ImportResult>();
        Assert.Equal(0, againResult!.Imported);
        Assert.Equal(3, againResult.AlreadyInBank);

        var bank = await client.GetFromJsonAsync<QuestionPage>("/api/questions");
        Assert.Equal(3, bank!.Total);
        Assert.All(bank.Questions, q => Assert.Equal("C#", q.TopicName));

        var after = await client.GetFromJsonAsync<List<LibraryItem>>("/api/library?topicName=C%23");
        Assert.Equal(3, after!.Count(i => i.AlreadyImported));
    }

    [Fact]
    public async Task Import_requires_a_selection()
    {
        var (client, _) = await factory.RegisterAsync();

        var res = await client.PostAsJsonAsync("/api/library/import", new { ids = Array.Empty<Guid>() });

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }
}
