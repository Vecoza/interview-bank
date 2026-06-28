namespace InterviewBank.API.Entities;

public enum SelfAssessment
{
    GotIt   = 1,
    Partial = 2,
    Missed  = 3
}

public class SessionQuestion
{
    public Guid Id { get; set; }

    public Guid SessionId { get; set; }

    public Guid QuestionId { get; set; }

    public int QuestionOrder { get; set; }

    public string? UserAnswer { get; set; }

    public int TimeTakenSeconds { get; set; }

    public SelfAssessment? SelfAssessment { get; set; }

    public MockInterviewSession Session { get; set; } = null!;
    public Question              Question { get; set; } = null!;
}
