namespace InterviewBank.API.DTOs.Library;

public class LibraryQuestionDto
{
    public Guid    Id              { get; set; }
    public string  TopicName       { get; set; } = string.Empty;
    public int     Difficulty      { get; set; }
    public string  Text            { get; set; } = string.Empty;
    public string? ExpectedAnswer  { get; set; }
    public bool    AlreadyImported { get; set; }
}

public class ImportLibraryQuestionsDto
{
    public List<Guid> Ids { get; set; } = [];
}

public class ImportLibraryQuestionsResultDto
{
    public int          Imported        { get; set; }
    public int          AlreadyInBank   { get; set; }
    public List<string> UnmatchedTopics { get; set; } = [];
}
