namespace RaceDay.API.Models;

public class Event
{
    public int EventId { get; set; }
    public int OrganiserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal? Distance { get; set; } // NEW: distance in km
    public string? RouteMap { get; set; }
    public string EventType { get; set; } = "Running"; // "Running", "Walking", "Cycling"
    public string Status { get; set; } = "Draft"; // "Draft", "Published", "Cancelled", "Completed"
    public int MaxParticipants { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Organiser Organiser { get; set; } = null!;
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    public ICollection<Weather> WeatherRecords { get; set; } = new List<Weather>();
}