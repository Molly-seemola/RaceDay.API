namespace RaceDay.API.Models;

public class User
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = "Participant"; // "Organiser" or "Participant"
    public DateTime DateRegistered { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public string? ProfileImageUrl { get; set; }

    // Navigation properties
    public Organiser? Organiser { get; set; }
    public Participant? Participant { get; set; }
}