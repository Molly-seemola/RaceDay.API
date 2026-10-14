using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Services;

public interface IWeatherService
{
    Task<IEnumerable<Weather>> GetWeatherByEventAsync(int eventId);
    Task<Weather> CreateWeatherAsync(int eventId, int organiserUserId, WeatherDto dto);
}