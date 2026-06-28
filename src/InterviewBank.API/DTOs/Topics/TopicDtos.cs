using System.ComponentModel.DataAnnotations;

namespace InterviewBank.API.DTOs.Topics;

public class TopicDto
{
    public Guid   Id            { get; set; }
    public string Name          { get; set; } = string.Empty;
    public bool   IsSystem      { get; set; }
    public int    QuestionCount { get; set; }
}

public class CreateTopicDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
