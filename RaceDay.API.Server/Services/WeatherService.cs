using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Services;

public class WeatherService : IWeatherService
{
    private readonly RaceDayDbContext _context;

    public WeatherService(RaceDayDbContext context)
    {
        _context = context;
    }

    // Get all weather records for an event
    public async Task<IEnumerable<Weather>> GetWeatherByEventAsync(int eventId)
    {
        return await _context.WeatherRecords
            .Where(w => w.EventId == eventId)
            .OrderBy(w => w.ForecastDate)
            .ToListAsync();
    }

    // Create a weather record for an event (Organiser only)
    public async Task<Weather> CreateWeatherAsync(int eventId, int organiserUserId, WeatherDto dto)
    {
        var eventEntity = await _context.Events
            .Include(e => e.Organiser)
            .FirstOrDefaultAsync(e => e.EventId == eventId);

        if (eventEntity == null)
            throw new KeyNotFoundException("Event not found");

        if (eventEntity.Organiser.UserId != organiserUserId)
            throw new UnauthorizedAccessException("You can only add weather to your own events");

        var weather = new Weather
        {
            EventId = eventId,
            ForecastDate = dto.ForecastDate,
            Temperature = dto.Temperature,
            Conditions = dto.Conditions,
            WindSpeed = dto.WindSpeed,
            Humidity = dto.Humidity,
            Precipitation = dto.Precipitation,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.WeatherRecords.Add(weather);
        await _context.SaveChangesAsync();
        return weather;
    }
}