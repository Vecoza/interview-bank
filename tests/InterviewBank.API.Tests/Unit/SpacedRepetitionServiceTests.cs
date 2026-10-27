using InterviewBank.API.Entities;
using InterviewBank.API.Services;

namespace InterviewBank.API.Tests.Unit;

public class SpacedRepetitionServiceTests
{
    private const int GotIt = 1, Partial = 2, Missed = 3;

    private static Question NewQuestion() => new() { EaseFactor = 2.5 };

    [Fact]
    public void First_correct_answer_schedules_review_tomorrow()
    {
        var q = NewQuestion();

        SpacedRepetitionService.Apply(q, GotIt);

        Assert.Equal(1, q.SrInterval);
        Assert.Equal(1, q.SrRepetitions);
        Assert.Equal(2.6, q.EaseFactor, 3);
        Assert.InRange(q.NextReviewAt!.Value, DateTimeOffset.UtcNow.AddDays(1).AddMinutes(-1),
                                               DateTimeOffset.UtcNow.AddDays(1).AddMinutes(1));
    }

    [Fact]
    public void Intervals_grow_one_six_then_by_ease_factor()
    {
        var q = NewQuestion();

        SpacedRepetitionService.Apply(q, GotIt);
        SpacedRepetitionService.Apply(q, GotIt);
        Assert.Equal(6, q.SrInterval);

        var easeBefore = q.EaseFactor;
        SpacedRepetitionService.Apply(q, GotIt);
        Assert.Equal((int)Math.Round(6 * easeBefore), q.SrInterval);
        Assert.Equal(3, q.SrRepetitions);
    }

    [Fact]
    public void Partial_answer_still_advances_but_lowers_ease()
    {
        var q = NewQuestion();

        SpacedRepetitionService.Apply(q, Partial);

        Assert.Equal(1, q.SrInterval);
        Assert.Equal(1, q.SrRepetitions);
        Assert.Equal(2.36, q.EaseFactor, 3);
    }

    [Fact]
    public void Missed_answer_resets_progress_and_is_due_immediately()
    {
        var q = NewQuestion();
        SpacedRepetitionService.Apply(q, GotIt);
        SpacedRepetitionService.Apply(q, GotIt);

        SpacedRepetitionService.Apply(q, Missed);

        Assert.Equal(0, q.SrRepetitions);
        Assert.Equal(1, q.SrInterval);
        Assert.True(q.NextReviewAt <= DateTimeOffset.UtcNow);
        Assert.Equal(2.5, q.EaseFactor, 3); // 2.5 -> 2.6 -> 2.7 -> 2.5
    }

    [Fact]
    public void Ease_factor_never_drops_below_minimum()
    {
        var q = NewQuestion();

        for (var i = 0; i < 20; i++)
            SpacedRepetitionService.Apply(q, Missed);

        Assert.Equal(1.3, q.EaseFactor, 3);
    }
}
