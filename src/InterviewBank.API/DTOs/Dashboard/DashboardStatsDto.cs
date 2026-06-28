namespace InterviewBank.API.DTOs.Dashboard;

public class DifficultyBreakdownDto
{
    public int Total     { get; set; }
    public int Practiced { get; set; }
}

public class TopicBreakdownDto
{
    public string TopicName { get; set; } = string.Empty;
    public int    Total     { get; set; }
    public int    Practiced { get; set; }
}

public class WeakTopicDto
{
    public string TopicName      { get; set; } = string.Empty;
    public int    MissedCount    { get; set; }
    public int    TotalAnswered  { get; set; }
    public double MissedRatio    { get; set; }
}

public class RecentSessionDto
{
    public Guid          SessionId      { get; set; }
    public DateTimeOffset CompletedAt   { get; set; }
    public int           TotalQuestions { get; set; }
    public int           GotItCount     { get; set; }
    public int           PartialCount   { get; set; }
    public int           MissedCount    { get; set; }
}

public class DashboardStatsDto
{
    public int TotalQuestions  { get; set; }
    public int PracticedCount  { get; set; }

    public Dictionary<string, DifficultyBreakdownDto> ByDifficulty { get; set; } = new();

    public List<TopicBreakdownDto> ByTopic { get; set; } = [];

    public List<RecentSessionDto> RecentSessions { get; set; } = [];

    public List<WeakTopicDto> WeakestTopics { get; set; } = [];

    public int PracticeStreak      { get; set; }
    public int DueForReviewCount   { get; set; }
}
