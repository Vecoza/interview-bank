using System.Security.Claims;
using InterviewBank.API.Data;
using InterviewBank.API.DTOs.MockInterview;
using InterviewBank.API.Entities;
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

        var session = await _service.StartSessionAsync(userId, dto, questions);

        return Ok(new ActiveSessionDto
        {
            SessionId       = session.Id,
            TimePerQuestion = dto.TimePerQuestion,
            Questions       = questions.Select((q, i) => new SessionQuestionDto
            {
                Id             = q.Id,
                Text           = q.Text,
                TopicName      = q.Topic.Name,
                Difficulty     = (int)q.Difficulty,
                QuestionOrder  = i + 1,
                QuestionType   = (int)q.QuestionType,
                CorrectAnswer  = q.QuestionType == Entities.QuestionType.YesNo ? q.ExpectedAnswer : null,
                ExpectedAnswer = q.ExpectedAnswer
            }).ToList()
        });
    }

    [HttpPost("complete")]
    public async Task<IActionResult> Complete([FromBody] CompleteSessionDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var session = await _service.CompleteSessionAsync(userId, dto.SessionId, dto.Answers);
        if (session is null)
            return BadRequest(new { error = "Session not found or already completed." });

        var results = await _db.SessionQuestions
            .Include(sq => sq.Question).ThenInclude(q => q.Topic)
            .Where(sq => sq.SessionId == session.Id)
            .OrderBy(sq => sq.QuestionOrder)
            .ToListAsync();

        var summary = new SessionSummaryDto
        {
            SessionId        = session.Id,
            StartedAt        = session.StartedAt,
            CompletedAt      = session.CompletedAt!.Value,
            TotalQuestions   = session.TotalQuestions,
            TimePerQuestion  = session.TimePerQuestion,
            GotItCount       = results.Count(r => r.SelfAssessment == SelfAssessment.GotIt),
            PartialCount     = results.Count(r => r.SelfAssessment == SelfAssessment.Partial),
            MissedCount      = results.Count(r => r.SelfAssessment == SelfAssessment.Missed),
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

    [HttpGet("due-count")]
    public async Task<IActionResult> GetDueCount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var now    = DateTimeOffset.UtcNow;

        var count = await _db.Questions
            .Where(q => q.UserId == userId &&
                        (q.NextReviewAt == null || q.NextReviewAt <= now))
            .CountAsync();

        return Ok(new { count });
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var sessions = await _db.MockInterviewSessions
            .Where(s => s.UserId == userId && s.CompletedAt != null)
            .OrderByDescending(s => s.CompletedAt)
            .Select(s => new PastSessionDto
            {
                SessionId      = s.Id,
                CompletedAt    = s.CompletedAt!.Value,
                TotalQuestions = s.TotalQuestions,
                GotItCount     = s.SessionQuestions.Count(sq => (int?)sq.SelfAssessment == 1),
                PartialCount   = s.SessionQuestions.Count(sq => (int?)sq.SelfAssessment == 2),
                MissedCount    = s.SessionQuestions.Count(sq => (int?)sq.SelfAssessment == 3)
            })
            .ToListAsync();

        return Ok(sessions);
    }
}
