using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public class WeatherDto
{
    [Required]
    public DateTime ForecastDate { get; set; }

    public decimal? Temperature { get; set; }
    public string? Conditions { get; set; }
    public decimal? WindSpeed { get; set; }
    public decimal? Humidity { get; set; }
    public decimal? Precipitation { get; set; }
}