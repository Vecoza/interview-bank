using InterviewBank.API.Data;
using InterviewBank.API.DTOs.MockInterview;
using InterviewBank.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace InterviewBank.API.Services;

public class MockInterviewService
{
    private readonly AppDbContext _db;

    public MockInterviewService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Question>> SelectQuestionsAsync(string userId, StartSessionDto dto)
    {
        var query = _db.Questions
            .Include(q => q.Topic)
            .Where(q => q.UserId == userId)
            .AsQueryable();

        if (dto.TopicIds?.Count > 0)
            query = query.Where(q => dto.TopicIds.Contains(q.TopicId));

        if (dto.Difficulties?.Count > 0)
            query = query.Where(q => dto.Difficulties.Contains((int)q.Difficulty));

        var now = DateTimeOffset.UtcNow;

        query = dto.Strategy switch
        {
            SelectionStrategy.LeastRecentlyPracticed =>
                query.OrderBy(q => q.LastPracticedAt ?? DateTimeOffset.MinValue),
            SelectionStrategy.HardestFirst =>
                query.OrderByDescending(q => q.Difficulty)
                     .ThenBy(_ => EF.Functions.Random()),
            SelectionStrategy.DueForReview =>
                query.Where(q => q.NextReviewAt == null || q.NextReviewAt <= now)
                     .OrderBy(q => q.NextReviewAt ?? DateTimeOffset.MinValue),
            _ => query.OrderBy(_ => EF.Functions.Random())
        };

        return await query.Take(dto.QuestionCount).ToListAsync();
    }

    /// <summary>
    /// Persists an in-progress session so it survives restarts and works across domains
    /// (no server-side cookie session needed).
    /// </summary>
    public async Task<MockInterviewSession> StartSessionAsync(string userId, StartSessionDto dto, List<Question> questions)
    {
        var session = new MockInterviewSession
        {
            Id              = Guid.NewGuid(),
            UserId          = userId,
            StartedAt       = DateTimeOffset.UtcNow,
            TotalQuestions  = questions.Count,
            TimePerQuestion = dto.TimePerQuestion
        };

        for (var i = 0; i < questions.Count; i++)
        {
            session.SessionQuestions.Add(new SessionQuestion
            {
                Id            = Guid.NewGuid(),
                SessionId     = session.Id,
                QuestionId    = questions[i].Id,
                QuestionOrder = i + 1
            });
        }

        _db.MockInterviewSessions.Add(session);
        await _db.SaveChangesAsync();
        return session;
    }

    /// <summary>
    /// Records answers for a started session and updates spaced-repetition state.
    /// Returns null when the session does not exist, belongs to someone else, or is already completed.
    /// </summary>
    public async Task<MockInterviewSession?> CompleteSessionAsync(string userId, Guid sessionId, List<SubmitAnswerDto> answers)
    {
        var session = await _db.MockInterviewSessions
            .Include(s => s.SessionQuestions)
                .ThenInclude(sq => sq.Question)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

        if (session is null || session.CompletedAt is not null)
            return null;

        var now       = DateTimeOffset.UtcNow;
        var byQuestion = session.SessionQuestions.ToDictionary(sq => sq.QuestionId);

        foreach (var a in answers)
        {
            // Ignore answers for questions that were not part of this session
            if (!byQuestion.TryGetValue(a.QuestionId, out var sq) || sq.SelfAssessment is not null)
                continue;

            sq.UserAnswer       = a.UserAnswer;
            sq.TimeTakenSeconds = Math.Min(a.TimeTakenSeconds, session.TimePerQuestion);
            sq.SelfAssessment   = (SelfAssessment)a.SelfAssessment;

            var question = sq.Question;
            question.PracticeCount++;
            question.LastPracticedAt = now;
            question.IsPracticed     = true;
            question.UpdatedAt       = now;

            SpacedRepetitionService.Apply(question, a.SelfAssessment);
        }

        // Questions the user never reached are dropped so they don't skew the stats
        var unanswered = session.SessionQuestions.Where(sq => sq.SelfAssessment is null).ToList();
        _db.SessionQuestions.RemoveRange(unanswered);

        session.TotalQuestions = session.SessionQuestions.Count - unanswered.Count;
        session.CompletedAt    = now;

        await _db.SaveChangesAsync();
        return session;
    }
}
