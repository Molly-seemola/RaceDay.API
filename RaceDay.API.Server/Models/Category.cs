namespace RaceDay.API.Models;

public class Category
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Distance { get; set; }
    public decimal EntryFee { get; set; }
    public TimeSpan? CategoryStartTime { get; set; }
    public int MaxParticipants { get; set; }
    public int CurrentCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Event Event { get; set; } = null!;
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}