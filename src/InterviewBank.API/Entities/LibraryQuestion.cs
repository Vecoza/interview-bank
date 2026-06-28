namespace InterviewBank.API.Entities;

public class LibraryQuestion
{
    public Guid       Id             { get; set; }
    public string     TopicName      { get; set; } = string.Empty;
    public Difficulty Difficulty     { get; set; }
    public string     Text           { get; set; } = string.Empty;
    public string?    ExpectedAnswer { get; set; }
}
