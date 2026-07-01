using System.Security.Claims;
using InterviewBank.API.Data;
using InterviewBank.API.DTOs.Library;
using InterviewBank.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InterviewBank.API.Controllers;

[ApiController]
[Route("api/library")]
[Authorize]
public class LibraryController : ControllerBase
{
    private readonly AppDbContext _db;

    public LibraryController(AppDbContext db) => _db = db;

    // GET /api/library?topicName=JavaScript&difficulty=1
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? topicName, [FromQuery] int? difficulty)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var query = _db.LibraryQuestions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(topicName))
            query = query.Where(q => q.TopicName == topicName);

        if (difficulty.HasValue)
            query = query.Where(q => (int)q.Difficulty == difficulty.Value);

        var importedIds = await _db.Questions
            .Where(q => q.UserId == userId && q.LibraryQuestionId != null)
            .Select(q => q.LibraryQuestionId!.Value)
            .ToHashSetAsync();

        var items = await query
            .OrderBy(q => q.TopicName)
            .ThenBy(q => q.Difficulty)
            .Select(q => new LibraryQuestionDto
            {
                Id             = q.Id,
                TopicName      = q.TopicName,
                Difficulty     = (int)q.Difficulty,
                Text           = q.Text,
                ExpectedAnswer = q.ExpectedAnswer
            })
            .ToListAsync();

        foreach (var item in items)
            item.AlreadyImported = importedIds.Contains(item.Id);

        return Ok(items);
    }

    // POST /api/library/import
    [HttpPost("import")]
    public async Task<IActionResult> Import([FromBody] ImportLibraryQuestionsDto dto)
    {
        if (dto.Ids.Count == 0)
            return BadRequest(new { error = "No questions selected." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var libraryItems = await _db.LibraryQuestions
            .Where(q => dto.Ids.Contains(q.Id))
            .ToListAsync();

        if (libraryItems.Count == 0)
            return BadRequest(new { error = "None of the selected questions were found." });

        var alreadyImportedIds = await _db.Questions
            .Where(q => q.UserId == userId && q.LibraryQuestionId != null && dto.Ids.Contains(q.LibraryQuestionId!.Value))
            .Select(q => q.LibraryQuestionId!.Value)
            .ToHashSetAsync();

        var topicNames = libraryItems.Select(q => q.TopicName).Distinct().ToList();
        var topics = await _db.Topics
            .Where(t => topicNames.Contains(t.Name))
            .ToDictionaryAsync(t => t.Name);

        var now = DateTimeOffset.UtcNow;
        var created = new List<Question>();
        var unmatchedTopics = new HashSet<string>();

        foreach (var item in libraryItems)
        {
            if (alreadyImportedIds.Contains(item.Id)) continue;

            if (!topics.TryGetValue(item.TopicName, out var topic))
            {
                unmatchedTopics.Add(item.TopicName);
                continue;
            }

            created.Add(new Question
            {
                Id                = Guid.NewGuid(),
                UserId            = userId,
                TopicId           = topic.Id,
                Text              = item.Text,
                Difficulty        = item.Difficulty,
                ExpectedAnswer    = item.ExpectedAnswer,
                Source            = "Library",
                LibraryQuestionId = item.Id,
                CreatedAt         = now,
                UpdatedAt         = now
            });
        }

        if (created.Count == 0 && alreadyImportedIds.Count == 0)
            return BadRequest(new { error = "Topics for the selected questions could not be matched." });

        _db.Questions.AddRange(created);
        await _db.SaveChangesAsync();

        return Ok(new ImportLibraryQuestionsResultDto
        {
            Imported        = created.Count,
            AlreadyInBank   = alreadyImportedIds.Count,
            UnmatchedTopics = unmatchedTopics.ToList()
        });
    }
}
