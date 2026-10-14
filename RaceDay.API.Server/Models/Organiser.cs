namespace RaceDay.API.Models;

public class Organiser
{
    public int OrganiserId { get; set; }
    public int UserId { get; set; }
    public string? CompanyName { get; set; }
    public string ContactNumber { get; set; } = string.Empty;
    public bool IsVerified { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<Event> Events { get; set; } = new List<Event>();
}