using Microsoft.AspNetCore.Mvc;
using RaceDay.API.DTOs;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    /// <summary>
    /// Get all events (public)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var events = await _eventService.GetAllEventsAsync();
        return Ok(events);
    }

    /// <summary>
    /// Get a single event by ID (public)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await _eventService.GetEventByIdAsync(id);
        if (ev == null)
            return NotFound(new { message = "Event not found" });

        return Ok(ev);
    }

    /// <summary>
    /// Get upcoming events (public)
    /// </summary>
    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming()
    {
        var events = await _eventService.GetUpcomingEventsAsync();
        return Ok(events);
    }

    /// <summary>
    /// Search events by name or location (public)
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { message = "Search query is required" });

        var events = await _eventService.SearchEventsAsync(q);
        return Ok(events);
    }

    /// <summary>
    /// Create a new event (Organiser only)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EventDto dto)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        if (userId == null)
            return Unauthorized(new { message = "Not logged in" });

        if (role != "Organiser")
            return Forbid();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var newEvent = await _eventService.CreateEventAsync(userId.Value, dto);
        return CreatedAtAction(nameof(GetById), new { id = newEvent.EventId }, newEvent);
    }

    /// <summary>
    /// Update an existing event (Organiser only — must own it)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EventDto dto)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        if (userId == null)
            return Unauthorized(new { message = "Not logged in" });

        if (role != "Organiser")
            return Forbid();

        var updated = await _eventService.UpdateEventAsync(id, userId.Value, dto);
        if (updated == null)
            return NotFound(new { message = "Event not found or not owned by you" });

        return Ok(updated);
    }

    /// <summary>
    /// Delete an event (Organiser only — must own it)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        if (userId == null)
            return Unauthorized(new { message = "Not logged in" });

        if (role != "Organiser")
            return Forbid();

        var deleted = await _eventService.DeleteEventAsync(id, userId.Value);
        if (!deleted)
            return NotFound(new { message = "Event not found or not owned by you" });

        return NoContent();
    }
}