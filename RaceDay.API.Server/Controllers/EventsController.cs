using Microsoft.AspNetCore.Mvc;          // Provides ControllerBase, IActionResult, HttpGet, HttpPost, etc.
using RaceDay.API.DTOs;                  // Contains EventDto used in the request bodies
using RaceDay.API.Services;              // Contains IEventService (business logic for events)

namespace RaceDay.API.Controllers;       // Groups this controller under the RaceDay.API.Controllers namespace

[ApiController]                          // Enables automatic model validation and API-specific behaviors
[Route("api/[controller]")]              // Route becomes /api/events  ([controller] = "Events")
public class EventsController : ControllerBase
{
    // Dependency-injected service that contains the business logic for events
    private readonly IEventService _eventService;

    /// <summary>
    /// Constructor injects the IEventService so we can use it in the endpoints below.
    /// </summary>
    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    /// <summary>
    /// GET /api/events
    /// Get all events (public).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // Call the service to fetch all events
        var events = await _eventService.GetAllEventsAsync();

        // Return 200 OK with the list of events
        return Ok(events);
    }

    /// <summary>
    /// GET /api/events/{id}
    /// Get a single event by ID (public).
    /// </summary>
    /// <param name="id">The event ID to retrieve</param>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        // Call the service to fetch a single event
        var ev = await _eventService.GetEventByIdAsync(id);

        // If the event does not exist → 404 Not Found
        if (ev == null)
            return NotFound(new { message = "Event not found" });

        // Return 200 OK with the event details
        return Ok(ev);
    }

    /// <summary>
    /// GET /api/events/upcoming
    /// Get upcoming events (public).
    /// </summary>
    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming()
    {
        // Call the service to fetch the upcoming events
        var events = await _eventService.GetUpcomingEventsAsync();

        // Return 200 OK with the list
        return Ok(events);
    }

    /// <summary>
    /// GET /api/events/search?q={query}
    /// Search events by name or location (public).
    /// </summary>
    /// <param name="q">The search query (name or location)</param>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        // Reject empty or whitespace-only searches → 400 Bad Request
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { message = "Search query is required" });

        // Call the service to search for matching events
        var events = await _eventService.SearchEventsAsync(q);

        // Return 200 OK with the matching events
        return Ok(events);
    }

    /// <summary>
    /// POST /api/events
    /// Create a new event (Organiser only).
    /// </summary>
    /// <param name="dto">Event data: name, description, date, location, etc.</param>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EventDto dto)
    {
        // Get the current user's ID and role from session
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        // If no session exists, the user is not logged in → 401
        if (userId == null)
            return Unauthorized(new { message = "Not logged in" });

        // Only Organisers are allowed to create events → 403 for anyone else
        if (role != "Organiser")
            return Forbid();

        // Validate the incoming DTO against any DataAnnotations
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Call the service to create the new event
        var newEvent = await _eventService.CreateEventAsync(userId.Value, dto);

        // Return 201 Created with the newly created event
        return CreatedAtAction(nameof(GetById), new { id = newEvent.EventId }, newEvent);
    }

    /// <summary>
    /// PUT /api/events/{id}
    /// Update an existing event (Organiser only — must own it).
    /// </summary>
    /// <param name="id">The event ID to update</param>
    /// <param name="dto">The updated event data</param>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EventDto dto)
    {
        // Get the current user's ID and role from session
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        // Reject unauthenticated requests → 401
        if (userId == null)
            return Unauthorized(new { message = "Not logged in" });

        // Only Organisers can update events → 403 for Participants
        if (role != "Organiser")
            return Forbid();

        // Call the service to update the event
        var updated = await _eventService.UpdateEventAsync(id, userId.Value, dto);

        // If the event doesn't exist OR isn't owned by this organiser → 404
        if (updated == null)
            return NotFound(new { message = "Event not found or not owned by you" });

        // Return 200 OK with the updated event
        return Ok(updated);
    }

    /// <summary>
    /// DELETE /api/events/{id}
    /// Delete an event (Organiser only — must own it).
    /// </summary>
    /// <param name="id">The event ID to delete</param>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Get the current user's ID and role from session
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        // Reject unauthenticated requests → 401
        if (userId == null)
            return Unauthorized(new { message = "Not logged in" });

        // Only Organisers can delete events → 403 for Participants
        if (role != "Organiser")
            return Forbid();

        // Call the service to delete the event
        var deleted = await _eventService.DeleteEventAsync(id, userId.Value);

        // If the event doesn't exist OR isn't owned by this organiser → 404
        if (!deleted)
            return NotFound(new { message = "Event not found or not owned by you" });

        // Return 204 No Content — successful deletion with no response body
        return NoContent();
    }
}