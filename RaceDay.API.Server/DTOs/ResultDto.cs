using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public class ResultDto
{
    [Required]
    public int EnrolmentId { get; set; }

    public TimeSpan? FinishTime { get; set; }
    public int? Position { get; set; }
    public int? OverallPosition { get; set; }
    public int? AgeCategoryPosition { get; set; }
    public string Status { get; set; } = "Confirmed";
}