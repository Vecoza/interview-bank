using InterviewBank.API.Services;

namespace InterviewBank.API.Tests.Infrastructure;

public class FakeAnswerEvaluator : IAnswerEvaluator
{
    public bool IsConfigured { get; set; } = true;

    public bool Fail { get; set; }

    public (string Question, string Expected, string Answer)? LastCall { get; private set; }

    public Task<AnswerEvaluation> EvaluateAsync(string question, string expectedAnswer, string userAnswer,
        CancellationToken ct = default)
    {
        LastCall = (question, expectedAnswer, userAnswer);

        if (Fail)
            throw new AnswerEvaluatorUnavailableException("AI service returned an error.");

        return Task.FromResult(new AnswerEvaluation(80, "Good answer, but mention the edge cases."));
    }
}
