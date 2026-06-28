using InterviewBank.API.Entities;

namespace InterviewBank.API.Services;

public static class SpacedRepetitionService
{
    // Maps our 1-3 self-assessment to SM-2 quality 0-5
    private static int ToQuality(int selfAssessment) => selfAssessment switch
    {
        1 => 5,  // Got it perfectly
        2 => 3,  // Partially (correct but with difficulty)
        _ => 1   // Missed it
    };

    public static void Apply(Question question, int selfAssessment)
    {
        var quality = ToQuality(selfAssessment);
        var now     = DateTimeOffset.UtcNow;

        if (quality >= 3)
        {
            question.SrInterval = question.SrRepetitions switch
            {
                0 => 1,
                1 => 6,
                _ => (int)Math.Round(question.SrInterval * question.EaseFactor)
            };

            question.EaseFactor = Math.Max(1.3,
                question.EaseFactor + 0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02));

            question.SrRepetitions++;
        }
        else
        {
            question.SrRepetitions = 0;
            question.SrInterval    = 1;
            question.EaseFactor    = Math.Max(1.3, question.EaseFactor - 0.2);
            // Missed questions are immediately due — show in the very next review session
            question.NextReviewAt  = now;
            return;
        }

        question.NextReviewAt = now.AddDays(question.SrInterval);
    }
}
