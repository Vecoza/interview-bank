using System.Security.Claims;
using InterviewBank.API.Data;
using InterviewBank.API.DTOs.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InterviewBank.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var questions = await _db.Questions
            .Include(q => q.Topic)
            .Where(q => q.UserId == userId)
            .ToListAsync();

        var sessions = await _db.MockInterviewSessions
            .Include(s => s.SessionQuestions)
                .ThenInclude(sq => sq.Question)
                    .ThenInclude(q => q.Topic)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CompletedAt)
            .Take(30)
            .ToListAsync();

        var byDifficulty = new Dictionary<string, DifficultyBreakdownDto>
        {
            ["Easy"]   = new() { Total = questions.Count(q => (int)q.Difficulty == 1), Practiced = questions.Count(q => (int)q.Difficulty == 1 && q.IsPracticed) },
            ["Medium"] = new() { Total = questions.Count(q => (int)q.Difficulty == 2), Practiced = questions.Count(q => (int)q.Difficulty == 2 && q.IsPracticed) },
            ["Hard"]   = new() { Total = questions.Count(q => (int)q.Difficulty == 3), Practiced = questions.Count(q => (int)q.Difficulty == 3 && q.IsPracticed) }
        };

        var byTopic = questions
            .GroupBy(q => q.Topic.Name)
            .Select(g => new TopicBreakdownDto
            {
                TopicName = g.Key,
                Total     = g.Count(),
                Practiced = g.Count(q => q.IsPracticed)
            })
            .OrderByDescending(t => t.Total)
            .ToList();

        var recentSessions = sessions
            .Take(10)
            .Select(s => new RecentSessionDto
            {
                SessionId      = s.Id,
                CompletedAt    = s.CompletedAt,
                TotalQuestions = s.TotalQuestions,
                GotItCount     = s.SessionQuestions.Count(sq => (int?)sq.SelfAssessment == 1),
                PartialCount   = s.SessionQuestions.Count(sq => (int?)sq.SelfAssessment == 2),
                MissedCount    = s.SessionQuestions.Count(sq => (int?)sq.SelfAssessment == 3)
            })
            .ToList();

        var weakestTopics = sessions
            .SelectMany(s => s.SessionQuestions)
            .Where(sq => sq.SelfAssessment.HasValue && sq.Question?.Topic != null)
            .GroupBy(sq => sq.Question.Topic.Name)
            .Select(g => new WeakTopicDto
            {
                TopicName     = g.Key,
                MissedCount   = g.Count(sq => (int)sq.SelfAssessment! == 3),
                TotalAnswered = g.Count(),
                MissedRatio   = g.Count() == 0 ? 0 : (double)g.Count(sq => (int)sq.SelfAssessment! == 3) / g.Count()
            })
            .Where(w => w.TotalAnswered >= 3)
            .OrderByDescending(w => w.MissedRatio)
            .Take(5)
            .ToList();

        var now = DateTimeOffset.UtcNow;

        return Ok(new DashboardStatsDto
        {
            TotalQuestions   = questions.Count,
            PracticedCount   = questions.Count(q => q.IsPracticed),
            ByDifficulty     = byDifficulty,
            ByTopic          = byTopic,
            RecentSessions   = recentSessions,
            WeakestTopics    = weakestTopics,
            PracticeStreak   = CalculateStreak(sessions),
            DueForReviewCount = questions.Count(q => q.NextReviewAt == null || q.NextReviewAt <= now)
        });
    }

    private static int CalculateStreak(List<Entities.MockInterviewSession> sessions)
    {
        if (sessions.Count == 0) return 0;

        var sessionDays = sessions
            .Select(s => s.CompletedAt.UtcDateTime.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        var today  = DateTime.UtcNow.Date;
        var streak = 0;
        var cursor = today;

        foreach (var day in sessionDays)
        {
            if (day == cursor || day == cursor.AddDays(-1))
            {
                streak++;
                cursor = day;
            }
            else
            {
                break;
            }
        }

        return streak;
    }
}
