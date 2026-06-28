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

    public async Task<MockInterviewSession> SaveSessionAsync(
        string userId,
        Guid sessionId,
        StartSessionDto config,
        DateTimeOffset startedAt,
        List<SubmitAnswerDto> answers)
    {
        var session = new MockInterviewSession
        {
            Id              = sessionId,
            UserId          = userId,
            StartedAt       = startedAt,
            CompletedAt     = DateTimeOffset.UtcNow,
            TotalQuestions  = answers.Count,
            TimePerQuestion = config.TimePerQuestion
        };

        _db.MockInterviewSessions.Add(session);

        foreach (var a in answers)
        {
            _db.SessionQuestions.Add(new SessionQuestion
            {
                Id               = Guid.NewGuid(),
                SessionId        = session.Id,
                QuestionId       = a.QuestionId,
                QuestionOrder    = answers.IndexOf(a) + 1,
                UserAnswer       = a.UserAnswer,
                TimeTakenSeconds = a.TimeTakenSeconds,
                SelfAssessment   = (SelfAssessment)a.SelfAssessment
            });

            var question = await _db.Questions.FindAsync(a.QuestionId);
            if (question is null || question.UserId != userId) continue;

            question.PracticeCount++;
            question.LastPracticedAt = DateTimeOffset.UtcNow;
            question.IsPracticed     = true;
            question.UpdatedAt       = DateTimeOffset.UtcNow;

            SpacedRepetitionService.Apply(question, a.SelfAssessment);
        }

        await _db.SaveChangesAsync();
        return session;
    }
}
