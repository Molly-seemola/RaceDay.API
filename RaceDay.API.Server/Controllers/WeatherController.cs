using Microsoft.AspNetCore.Mvc;
using RaceDay.API.DTOs;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;

    public WeatherController(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    /// <summary>
    /// Get weather for an event (public)
    /// </summary>
    [HttpGet("weather/event/{eventId}")]
    public async Task<IActionResult> GetByEvent(int eventId)
    {
        var weather = await _weatherService.GetWeatherByEventAsync(eventId);
        if (!weather.Any())
            return NotFound(new { message = "No weather data for this event" });

        return Ok(weather);
    }

    /// <summary>
    /// Add or update weather for an event (Organiser only)
    /// </summary>
    [HttpPost("weather/event/{eventId}")]
    public async Task<IActionResult> Create(int eventId, [FromBody] WeatherDto dto)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        if (userId == null)
            return Unauthorized(new { message = "Not logged in" });

        if (role != "Organiser")
            return Forbid();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var weather = await _weatherService.CreateWeatherAsync(eventId, userId.Value, dto);
            return CreatedAtAction(nameof(GetByEvent), new { eventId = eventId }, weather);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}