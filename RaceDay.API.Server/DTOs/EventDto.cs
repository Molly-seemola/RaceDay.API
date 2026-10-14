using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public class EventDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime EventDate { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    public TimeSpan? EndTime { get; set; }

    [Required]
    [MaxLength(255)]
    public string Location { get; set; } = string.Empty;

    [Range(0, 1000)]
    public decimal? Distance { get; set; }

    [MaxLength(500)]
    public string? RouteMap { get; set; }

    [Required]
    [RegularExpression("^(Running|Walking|Cycling)$")]
    public string EventType { get; set; } = string.Empty;

    [RegularExpression("^(Draft|Published|Cancelled|Completed)$")]
    public string Status { get; set; } = "Draft";

    [Range(1, 100000)]
    public int MaxParticipants { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }
}