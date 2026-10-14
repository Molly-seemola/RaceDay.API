namespace RaceDay.API.Models;

public class Weather
{
    public int WeatherId { get; set; }
    public int EventId { get; set; }
    public DateTime ForecastDate { get; set; }
    public decimal? Temperature { get; set; }
    public string? Conditions { get; set; }
    public decimal? WindSpeed { get; set; }
    public decimal? Humidity { get; set; }
    public decimal? Precipitation { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Event Event { get; set; } = null!;
}