using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Organiser|Participant)$",
        ErrorMessage = "Role must be either 'Organiser' or 'Participant'")]
    public string Role { get; set; } = string.Empty;

    // Optional fields for Organiser
    public string? CompanyName { get; set; }
    public string? ContactNumber { get; set; }

    // Optional fields for Participant
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
}