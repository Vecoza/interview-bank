using System.Security.Claims;
using InterviewBank.API.Data;
using InterviewBank.API.DTOs.MockInterview;
using InterviewBank.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InterviewBank.API.Controllers;

[ApiController]
[Route("api/mock-interview")]
[Authorize]
public class MockInterviewController : ControllerBase
{
    private readonly MockInterviewService _service;
    private readonly AppDbContext _db;

    public MockInterviewController(MockInterviewService service, AppDbContext db)
    {
        _service = service;
        _db      = db;
    }

    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] StartSessionDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var questions = await _service.SelectQuestionsAsync(userId, dto);

        if (questions.Count == 0)
            return BadRequest(new { error = "No questions match the selected filters." });

        var sessionId = Guid.NewGuid();

        var response = new ActiveSessionDto
        {
            SessionId       = sessionId,
            TimePerQuestion = dto.TimePerQuestion,
            Questions       = questions.Select((q, i) => new SessionQuestionDto
            {
                Id            = q.Id,
                Text          = q.Text,
                TopicName     = q.Topic.Name,
                Difficulty    = (int)q.Difficulty,
                QuestionOrder = i + 1
            }).ToList()
        };

        HttpContext.Session.SetString($"session:{sessionId}:config", System.Text.Json.JsonSerializer.Serialize(dto));
        HttpContext.Session.SetString($"session:{sessionId}:startedAt", DateTimeOffset.UtcNow.ToString("O"));

        return Ok(response);
    }

    [HttpPost("complete")]
    public async Task<IActionResult> Complete([FromBody] CompleteSessionDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var configJson  = HttpContext.Session.GetString($"session:{dto.SessionId}:config");
        var startedAtStr = HttpContext.Session.GetString($"session:{dto.SessionId}:startedAt");

        if (configJson is null || startedAtStr is null)
            return BadRequest(new { error = "Session not found or already completed." });

        var config    = System.Text.Json.JsonSerializer.Deserialize<StartSessionDto>(configJson)!;
        var startedAt = DateTimeOffset.Parse(startedAtStr);

        var session = await _service.SaveSessionAsync(userId, dto.SessionId, config, startedAt, dto.Answers);

        HttpContext.Session.Remove($"session:{dto.SessionId}:config");
        HttpContext.Session.Remove($"session:{dto.SessionId}:startedAt");

        var results = await _db.SessionQuestions
            .Include(sq => sq.Question).ThenInclude(q => q.Topic)
            .Where(sq => sq.SessionId == session.Id)
            .OrderBy(sq => sq.QuestionOrder)
            .ToListAsync();

        var summary = new SessionSummaryDto
        {
            SessionId        = session.Id,
            StartedAt        = session.StartedAt,
            CompletedAt      = session.CompletedAt,
            TotalQuestions   = session.TotalQuestions,
            TimePerQuestion  = session.TimePerQuestion,
            GotItCount       = results.Count(r => (int?)r.SelfAssessment == 1),
            PartialCount     = results.Count(r => (int?)r.SelfAssessment == 2),
            MissedCount      = results.Count(r => (int?)r.SelfAssessment == 3),
            AverageTimeTaken = results.Any() ? results.Average(r => r.TimeTakenSeconds) : 0,
            Results          = results.Select(r => new SessionResultItemDto
            {
                QuestionText     = r.Question.Text,
                TopicName        = r.Question.Topic.Name,
                Difficulty       = (int)r.Question.Difficulty,
                UserAnswer       = r.UserAnswer,
                ExpectedAnswer   = r.Question.ExpectedAnswer,
                TimeTakenSeconds = r.TimeTakenSeconds,
                SelfAssessment   = r.SelfAssessment.HasValue ? (int)r.SelfAssessment : 0,
                QuestionOrder    = r.QuestionOrder
            }).ToList()
        };

        return Ok(summary);
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var sessions = await _db.MockInterviewSessions
            .Include(s => s.SessionQuestions)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CompletedAt)
            .Select(s => new PastSessionDto
            {
                SessionId      = s.Id,
                CompletedAt    = s.CompletedAt,
                TotalQuestions = s.TotalQuestions,
                GotItCount     = s.SessionQuestions.Count(sq => (int?)sq.SelfAssessment == 1),
                PartialCount   = s.SessionQuestions.Count(sq => (int?)sq.SelfAssessment == 2),
                MissedCount    = s.SessionQuestions.Count(sq => (int?)sq.SelfAssessment == 3)
            })
            .ToListAsync();

        return Ok(sessions);
    }
}
