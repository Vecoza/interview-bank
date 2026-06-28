namespace InterviewBank.API.Entities;

public class MockInterviewSession
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset CompletedAt { get; set; }

    public int TotalQuestions { get; set; }

    public int TimePerQuestion { get; set; }

    public ICollection<SessionQuestion> SessionQuestions { get; set; } = new List<SessionQuestion>();
}
