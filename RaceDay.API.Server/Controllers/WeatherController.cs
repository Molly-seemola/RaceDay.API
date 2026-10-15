using Microsoft.AspNetCore.Mvc;         
using RaceDay.API.DTOs;                  
using RaceDay.API.Services;              

namespace RaceDay.API.Controllers;       

/// <summary>
/// Controller that handles all Weather-related API operations.
/// Supports two behaviours:
///   - Public users: can view weather for any event
///   - Organisers: can add weather data to their own events
/// </summary>
[ApiController]                          // Enables automatic model validation and API-specific behaviors
[Route("api")]                           // Base route for this controller, e.g. /api/weather/event/{eventId}
public class WeatherController : ControllerBase
{
    // Dependency-injected service that contains the business logic for weather
    private readonly IWeatherService _weatherService;

    /// <summary>
    /// Constructor injects the IWeatherService so we can use it in the endpoints below.
    /// </summary>
    public WeatherController(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    /// <summary>
    /// GET /api/weather/event/{eventId}
    /// Get weather records for a specific event.
    /// Public endpoint — no login required.
    /// </summary>
    /// <param name="eventId">The ID of the event whose weather is being requested</param>
    [HttpGet("weather/event/{eventId}")]
    public async Task<IActionResult> GetByEvent(int eventId)
    {
        // Call the service to fetch all weather records for this event
        var weather = await _weatherService.GetWeatherByEventAsync(eventId);

        // If there are no weather records for this event, return 404 Not Found
        if (!weather.Any())
            return NotFound(new { message = "No weather data for this event" });

        // Return 200 OK with the weather records
        return Ok(weather);
    }

    /// <summary>
    /// POST /api/weather/event/{eventId}
    /// Add new weather data for an event (Organiser only).
    /// Only the organiser who owns the event can add weather to it.
    /// </summary>
    /// <param name="eventId">The event ID to attach weather data to</param>
    /// <param name="dto">The weather data (forecast date, temperature, conditions, etc.)</param>
    [HttpPost("weather/event/{eventId}")]
    public async Task<IActionResult> Create(int eventId, [FromBody] WeatherDto dto)
    {
        // Get the current user's ID and role from session
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        // If no session exists, the user is not logged in → 401
        if (userId == null)
            return Unauthorized(new { message = "Not logged in" });

        // Only Organisers are allowed to add weather data → 403 for anyone else
        if (role != "Organiser")
            return Forbid();

        // Validate the incoming DTO against any DataAnnotations
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // Call the service to persist the new weather record
            var weather = await _weatherService.CreateWeatherAsync(eventId, userId.Value, dto);

            // Return 201 Created with the newly created weather record
            return CreatedAtAction(nameof(GetByEvent), new { eventId = eventId }, weather);
        }
        catch (KeyNotFoundException ex)
        {
            // Thrown by the service when the event does not exist → 404 Not Found
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            // Thrown by the service when the organiser does not own the event → 403 Forbidden
            return Forbid(ex.Message);
        }
    }
}