using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public class CategoryDto
{
    [Required]
    [MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [Range(0, 1000)]
    public decimal Distance { get; set; }

    [Range(0, 100000)]
    public decimal EntryFee { get; set; }

    public TimeSpan? CategoryStartTime { get; set; }

    [Range(1, 100000)]
    public int MaxParticipants { get; set; }
}
