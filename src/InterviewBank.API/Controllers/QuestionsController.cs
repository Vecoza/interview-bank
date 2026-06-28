using System.Security.Claims;
using InterviewBank.API.Data;
using InterviewBank.API.DTOs.Questions;
using InterviewBank.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InterviewBank.API.Controllers;

[ApiController]
[Route("api/questions")]
[Authorize]
public class QuestionsController : ControllerBase
{
    private readonly AppDbContext _db;

    public QuestionsController(AppDbContext db) => _db = db;

    // GET /api/questions?search=&topicIds=&difficulties=&isPracticed=&page=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] QuestionFilterDto filter)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var query = _db.Questions
            .Include(q => q.Topic)
            .Where(q => q.UserId == userId)
            .AsQueryable();

        // ── Filters (AND logic) ───────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(q => EF.Functions.ILike(q.Text, $"%{filter.Search.Trim()}%"));

        if (filter.TopicIds?.Count > 0)
            query = query.Where(q => filter.TopicIds.Contains(q.TopicId));

        if (filter.Difficulties?.Count > 0)
            query = query.Where(q => filter.Difficulties.Contains((int)q.Difficulty));

        if (filter.IsPracticed.HasValue)
            query = query.Where(q => q.IsPracticed == filter.IsPracticed.Value);

        // ── Pagination ────────────────────────────────────────────────────────
        var total = await query.CountAsync();

        var questions = await query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(q => QuestionDto.From(q))
            .ToListAsync();

        return Ok(new PaginatedQuestionsDto
        {
            Total     = total,
            Page      = filter.Page,
            PageSize  = filter.PageSize,
            Questions = questions
        });
    }

    // GET /api/questions/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var q = await _db.Questions
            .Include(q => q.Topic)
            .FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId);

        if (q is null) return NotFound();

        return Ok(QuestionDto.From(q));
    }

    // POST /api/questions
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQuestionDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var topicExists = await _db.Topics.AnyAsync(t => t.Id == dto.TopicId);
        if (!topicExists)
            return BadRequest(new { error = "Topic not found." });

        var question = new Question
        {
            Id             = Guid.NewGuid(),
            UserId         = userId,
            TopicId        = dto.TopicId,
            Text           = dto.Text.Trim(),
            Difficulty     = (Difficulty)dto.Difficulty,
            ExpectedAnswer = dto.ExpectedAnswer,
            PersonalNotes  = dto.PersonalNotes,
            Source         = dto.Source,
            CreatedAt      = DateTimeOffset.UtcNow,
            UpdatedAt      = DateTimeOffset.UtcNow
        };

        _db.Questions.Add(question);
        await _db.SaveChangesAsync();

        // Re-load with topic for the response
        await _db.Entry(question).Reference(q => q.Topic).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = question.Id }, QuestionDto.From(question));
    }

    // PUT /api/questions/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateQuestionDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var question = await _db.Questions
            .Include(q => q.Topic)
            .FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId);

        if (question is null) return NotFound();

        var topicExists = await _db.Topics.AnyAsync(t => t.Id == dto.TopicId);
        if (!topicExists)
            return BadRequest(new { error = "Topic not found." });

        question.Text           = dto.Text.Trim();
        question.TopicId        = dto.TopicId;
        question.Difficulty     = (Difficulty)dto.Difficulty;
        question.ExpectedAnswer = dto.ExpectedAnswer;
        question.PersonalNotes  = dto.PersonalNotes;
        question.Source         = dto.Source;
        question.UpdatedAt      = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();
        await _db.Entry(question).Reference(q => q.Topic).LoadAsync();

        return Ok(QuestionDto.From(question));
    }

    // DELETE /api/questions/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var question = await _db.Questions
            .FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId);

        if (question is null) return NotFound();

        _db.Questions.Remove(question);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // PATCH /api/questions/{id}/practiced
    [HttpPatch("{id:guid}/practiced")]
    public async Task<IActionResult> TogglePracticed(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var question = await _db.Questions
            .Include(q => q.Topic)
            .FirstOrDefaultAsync(q => q.Id == id && q.UserId == userId);

        if (question is null) return NotFound();

        question.IsPracticed = !question.IsPracticed;
        question.UpdatedAt   = DateTimeOffset.UtcNow;

        if (question.IsPracticed)
        {
            question.PracticeCount++;
            question.LastPracticedAt = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync();

        return Ok(QuestionDto.From(question));
    }
}
