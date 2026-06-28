using System.ComponentModel.DataAnnotations;
using InterviewBank.API.Entities;

namespace InterviewBank.API.DTOs.Questions;

// ── Query / filter ────────────────────────────────────────────────────────────

public class QuestionFilterDto
{
    public string?        Search       { get; set; }
    public List<Guid>?    TopicIds     { get; set; }
    public List<int>?     Difficulties { get; set; }
    public bool?          IsPracticed  { get; set; }

    public int Page     { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

// ── Write ─────────────────────────────────────────────────────────────────────

public class CreateQuestionDto
{
    [Required]
    [MaxLength(1000)]
    public string Text { get; set; } = string.Empty;

    [Required]
    public Guid TopicId { get; set; }

    [Required]
    [Range(1, 3)]
    public int Difficulty { get; set; }

    public string? ExpectedAnswer { get; set; }
    public string? PersonalNotes  { get; set; }

    [MaxLength(500)]
    public string? Source { get; set; }
}

public class UpdateQuestionDto
{
    [Required]
    [MaxLength(1000)]
    public string Text { get; set; } = string.Empty;

    [Required]
    public Guid TopicId { get; set; }

    [Required]
    [Range(1, 3)]
    public int Difficulty { get; set; }

    public string? ExpectedAnswer { get; set; }
    public string? PersonalNotes  { get; set; }

    [MaxLength(500)]
    public string? Source { get; set; }
}

// ── Read ──────────────────────────────────────────────────────────────────────

public class QuestionDto
{
    public Guid    Id              { get; set; }
    public string  UserId          { get; set; } = string.Empty;
    public Guid    TopicId         { get; set; }
    public string  TopicName       { get; set; } = string.Empty;
    public string  Text            { get; set; } = string.Empty;
    public int     Difficulty      { get; set; }
    public string  DifficultyLabel { get; set; } = string.Empty;
    public string? ExpectedAnswer  { get; set; }
    public string? PersonalNotes   { get; set; }
    public string? Source          { get; set; }
    public bool    IsPracticed     { get; set; }
    public int     PracticeCount   { get; set; }
    public DateTimeOffset? LastPracticedAt { get; set; }
    public DateTimeOffset  CreatedAt       { get; set; }
    public DateTimeOffset  UpdatedAt       { get; set; }

    public static QuestionDto From(Entities.Question q) => new()
    {
        Id              = q.Id,
        UserId          = q.UserId,
        TopicId         = q.TopicId,
        TopicName       = q.Topic.Name,
        Text            = q.Text,
        Difficulty      = (int)q.Difficulty,
        DifficultyLabel = q.Difficulty.ToString(),
        ExpectedAnswer  = q.ExpectedAnswer,
        PersonalNotes   = q.PersonalNotes,
        Source          = q.Source,
        IsPracticed     = q.IsPracticed,
        PracticeCount   = q.PracticeCount,
        LastPracticedAt = q.LastPracticedAt,
        CreatedAt       = q.CreatedAt,
        UpdatedAt       = q.UpdatedAt
    };
}

public class PaginatedQuestionsDto
{
    public int                 Total     { get; set; }
    public int                 Page      { get; set; }
    public int                 PageSize  { get; set; }
    public List<QuestionDto>   Questions { get; set; } = [];
}
