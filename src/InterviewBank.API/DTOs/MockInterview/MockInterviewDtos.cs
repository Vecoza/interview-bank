using System.ComponentModel.DataAnnotations;

namespace InterviewBank.API.DTOs.MockInterview;

public enum SelectionStrategy
{
    Random = 0,
    LeastRecentlyPracticed = 1,
    HardestFirst = 2
}

public class StartSessionDto
{
    [Required]
    [Range(5, 20)]
    public int QuestionCount { get; set; }

    public List<Guid>? TopicIds { get; set; }

    public List<int>? Difficulties { get; set; }

    [Required]
    public SelectionStrategy Strategy { get; set; }

    [Required]
    [Range(30, 180)]
    public int TimePerQuestion { get; set; }
}

public class SessionQuestionDto
{
    public Guid   Id            { get; set; }
    public string Text          { get; set; } = string.Empty;
    public string TopicName     { get; set; } = string.Empty;
    public int    Difficulty    { get; set; }
    public int    QuestionOrder { get; set; }
}

public class SubmitAnswerDto
{
    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public Guid QuestionId { get; set; }

    public string? UserAnswer { get; set; }

    [Required]
    [Range(0, 180)]
    public int TimeTakenSeconds { get; set; }

    [Required]
    [Range(1, 3)]
    public int SelfAssessment { get; set; }
}

public class CompleteSessionDto
{
    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public List<SubmitAnswerDto> Answers { get; set; } = [];
}

public class ActiveSessionDto
{
    public Guid                    SessionId       { get; set; }
    public int                     TimePerQuestion { get; set; }
    public List<SessionQuestionDto> Questions      { get; set; } = [];
}

public class SessionResultItemDto
{
    public string  QuestionText     { get; set; } = string.Empty;
    public string  TopicName        { get; set; } = string.Empty;
    public int     Difficulty       { get; set; }
    public string? UserAnswer       { get; set; }
    public string? ExpectedAnswer   { get; set; }
    public int     TimeTakenSeconds { get; set; }
    public int     SelfAssessment   { get; set; }
    public int     QuestionOrder    { get; set; }
}

public class SessionSummaryDto
{
    public Guid                      SessionId         { get; set; }
    public DateTimeOffset            StartedAt         { get; set; }
    public DateTimeOffset            CompletedAt       { get; set; }
    public int                       TotalQuestions    { get; set; }
    public int                       TimePerQuestion   { get; set; }
    public int                       GotItCount        { get; set; }
    public int                       PartialCount      { get; set; }
    public int                       MissedCount       { get; set; }
    public double                    AverageTimeTaken  { get; set; }
    public List<SessionResultItemDto> Results          { get; set; } = [];
}

public class PastSessionDto
{
    public Guid          SessionId      { get; set; }
    public DateTimeOffset CompletedAt   { get; set; }
    public int           TotalQuestions { get; set; }
    public int           GotItCount     { get; set; }
    public int           PartialCount   { get; set; }
    public int           MissedCount    { get; set; }
}
