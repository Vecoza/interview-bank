namespace InterviewBank.API.Entities;

public enum Difficulty
{
    Easy   = 1,
    Medium = 2,
    Hard   = 3
}

public enum QuestionType
{
    Essay = 0,
    YesNo = 1
}

public class Question
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public Guid TopicId { get; set; }

    public string Text { get; set; } = string.Empty;

    public Difficulty    Difficulty    { get; set; }

    public QuestionType  QuestionType  { get; set; } = QuestionType.Essay;

    public string? ExpectedAnswer { get; set; }

    public string? PersonalNotes { get; set; }

    public string? Source { get; set; }

    public bool IsPracticed { get; set; }

    public int PracticeCount { get; set; }

    public DateTimeOffset? LastPracticedAt { get; set; }

    public double           EaseFactor    { get; set; } = 2.5;
    public int              SrInterval    { get; set; } = 0;
    public int              SrRepetitions { get; set; } = 0;
    public DateTimeOffset?  NextReviewAt  { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Topic Topic { get; set; } = null!;
}
