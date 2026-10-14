namespace RaceDay.API.Models;

public class Result
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? Position { get; set; }
    public int? OverallPosition { get; set; }
    public int? AgeCategoryPosition { get; set; }
    public string Status { get; set; } = "Pending"; // "Pending", "Confirmed", "Disqualified"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Enrolment Enrolment { get; set; } = null!;
}