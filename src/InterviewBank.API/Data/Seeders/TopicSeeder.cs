using InterviewBank.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace InterviewBank.API.Data.Seeders;

public static class TopicSeeder
{
    private static readonly string[] SystemTopics =
    [
        "JavaScript", "TypeScript", "C#", ".NET", "Angular", "React",
        "SQL", "PostgreSQL", "System Design", "Algorithms",
        "Data Structures", "Behavioral", "General"
    ];

    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();

        var existing = await db.Topics
            .Where(t => t.IsSystem)
            .Select(t => t.Name)
            .ToHashSetAsync();

        var toInsert = SystemTopics
            .Where(name => !existing.Contains(name))
            .Select(name => new Topic
            {
                Id = Guid.NewGuid(),
                Name = name,
                IsSystem = true
            })
            .ToList();

        if (toInsert.Count > 0)
        {
            db.Topics.AddRange(toInsert);
            await db.SaveChangesAsync();
        }
    }
}
