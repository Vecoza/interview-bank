namespace InterviewBank.API.Entities;

public class Topic
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsSystem { get; set; }

    public string? CreatedByUserId { get; set; }

    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
