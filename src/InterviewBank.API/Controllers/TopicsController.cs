using System.Security.Claims;
using InterviewBank.API.Data;
using InterviewBank.API.DTOs.Topics;
using InterviewBank.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InterviewBank.API.Controllers;

[ApiController]
[Route("api/topics")]
[Authorize]
public class TopicsController : ControllerBase
{
    private readonly AppDbContext _db;

    public TopicsController(AppDbContext db) => _db = db;

    // GET /api/topics
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var topics = await _db.Topics
            .OrderBy(t => t.IsSystem ? 0 : 1)   // system topics first
            .ThenBy(t => t.Name)
            .Select(t => new TopicDto
            {
                Id            = t.Id,
                Name          = t.Name,
                IsSystem      = t.IsSystem,
                // Only count this user's questions per topic
                QuestionCount = t.Questions.Count(q => q.UserId == userId)
            })
            .ToListAsync();

        return Ok(topics);
    }

    // POST /api/topics
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTopicDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var exists = await _db.Topics.AnyAsync(t =>
            t.Name.ToLower() == dto.Name.ToLower());

        if (exists)
            return Conflict(new { error = "A topic with that name already exists." });

        var topic = new Topic
        {
            Id              = Guid.NewGuid(),
            Name            = dto.Name.Trim(),
            IsSystem        = false,
            CreatedByUserId = userId
        };

        _db.Topics.Add(topic);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new TopicDto
        {
            Id            = topic.Id,
            Name          = topic.Name,
            IsSystem      = false,
            QuestionCount = 0
        });
    }

    // DELETE /api/topics/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var topic = await _db.Topics.FindAsync(id);
        if (topic is null) return NotFound();

        if (topic.IsSystem)
            return BadRequest(new { error = "System topics cannot be deleted." });

        if (topic.CreatedByUserId != userId)
            return Forbid();

        _db.Topics.Remove(topic);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
