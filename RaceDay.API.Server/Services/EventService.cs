using Microsoft.EntityFrameworkCore;      
using RaceDay.API.Data;                    
using RaceDay.API.DTOs;                    
using RaceDay.API.Models;                  // Contains entity classes (Event, Organiser, etc.)

namespace RaceDay.API.Services;            // Groups this class under RaceDay.API.Services

/// <summary>
/// Service that handles all Event-related business logic:
///   - Fetching events (all, by ID, upcoming, search)
///   - Creating, updating, and deleting events
///   - Enforcing that only the owner organiser can modify their events
/// </summary>
public class EventService : IEventService
{
    // Database context used for all database operations
    private readonly RaceDayDbContext _context;

    /// <summary>
    /// Constructor injects the database context.
    /// </summary>
    public EventService(RaceDayDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all events from the database.
    /// Includes the Organiser and their User details so they appear in the response.
    /// Sorted by event date descending (most recent first).
    /// </summary>
    public async Task<IEnumerable<Event>> GetAllEventsAsync()
    {
        return await _context.Events
            .Include(e => e.Organiser).ThenInclude(o => o.User)   // Eager load Organiser + User
            .OrderByDescending(e => e.EventDate)                  // Newest events first
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a single event by its ID.
    /// Includes the Organiser, User, and all Categories of the event.
    /// </summary>
    /// <param name="id">The event ID</param>
    /// <returns>The event or null if not found</returns>
    public async Task<Event?> GetEventByIdAsync(int id)
    {
        return await _context.Events
            .Include(e => e.Organiser).ThenInclude(o => o.User)   // Load organiser info
            .Include(e => e.Categories)                           // Load related categories
            .FirstOrDefaultAsync(e => e.EventId == id);
    }

    /// <summary>
    /// Creates a new event.
    /// Looks up the Organiser profile for the logged-in user ID.
    /// Throws an exception if the user does not have an Organiser profile.
    /// </summary>
    /// <param name="organiserId">The UserId of the logged-in organiser</param>
    /// <param name="dto">The event data from the client</param>
    /// <returns>The created Event</returns>
    public async Task<Event> CreateEventAsync(int organiserId, EventDto dto)
    {
        // Find the Organiser profile linked to this User
        var organiser = await _context.Organisers
            .FirstOrDefaultAsync(o => o.UserId == organiserId);

        // If no organiser profile exists, the user isn't allowed to create events
        if (organiser == null)
            throw new UnauthorizedAccessException("Organiser profile not found");

        // Build the new Event entity from the DTO
        var eventEntity = new Event
        {
            OrganiserId = organiser.OrganiserId,   // Link to the owner's OrganiserId
            Name = dto.Name,
            Description = dto.Description,
            EventDate = dto.EventDate,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Location = dto.Location,
            Distance = dto.Distance,
            RouteMap = dto.RouteMap,
            EventType = dto.EventType,
            Status = dto.Status,
            MaxParticipants = dto.MaxParticipants,
            ImageUrl = dto.ImageUrl
        };

        // Save the new event
        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();
        return eventEntity;
    }

    /// <summary>
    /// Updates an existing event.
    /// Only the organiser who owns the event can update it.
    /// Returns null if the event doesn't exist or if the caller is not the owner.
    /// </summary>
    /// <param name="id">The event ID to update</param>
    /// <param name="organiserId">The UserId of the logged-in organiser</param>
    /// <param name="dto">The updated event data</param>
    /// <returns>The updated Event or null</returns>
    public async Task<Event?> UpdateEventAsync(int id, int organiserId, EventDto dto)
    {
        // Load the event with its organiser
        var eventEntity = await _context.Events
            .Include(e => e.Organiser)
            .FirstOrDefaultAsync(e => e.EventId == id);

        // Return null if event doesn't exist
        if (eventEntity == null) return null;

        // Ownership check: only the owner organiser can modify this event
        if (eventEntity.Organiser.UserId != organiserId) return null;

        // Update all fields from the DTO
        eventEntity.Name = dto.Name;
        eventEntity.Description = dto.Description;
        eventEntity.EventDate = dto.EventDate;
        eventEntity.StartTime = dto.StartTime;
        eventEntity.EndTime = dto.EndTime;
        eventEntity.Location = dto.Location;
        eventEntity.Distance = dto.Distance;
        eventEntity.RouteMap = dto.RouteMap;
        eventEntity.EventType = dto.EventType;
        eventEntity.Status = dto.Status;
        eventEntity.MaxParticipants = dto.MaxParticipants;
        eventEntity.ImageUrl = dto.ImageUrl;
        eventEntity.UpdatedAt = DateTime.UtcNow;   // Record the modification time

        // Save changes
        await _context.SaveChangesAsync();
        return eventEntity;
    }

    /// <summary>
    /// Deletes an event.
    /// Only the organiser who owns the event can delete it.
    /// Returns false if the event doesn't exist or the caller is not the owner.
    /// </summary>
    /// <param name="id">The event ID to delete</param>
    /// <param name="organiserId">The UserId of the logged-in organiser</param>
    /// <returns>True if deleted, false otherwise</returns>
    public async Task<bool> DeleteEventAsync(int id, int organiserId)
    {
        // Load the event with its organiser
        var eventEntity = await _context.Events
            .Include(e => e.Organiser)
            .FirstOrDefaultAsync(e => e.EventId == id);

        // Return false if event doesn't exist
        if (eventEntity == null) return false;

        // Ownership check: only the owner organiser can delete this event
        if (eventEntity.Organiser.UserId != organiserId) return false;

        // Remove the event (cascade deletes related categories, enrolments, etc.)
        _context.Events.Remove(eventEntity);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Retrieves the next 10 upcoming published events.
    /// Only returns events whose date is today or later AND whose status is "Published".
    /// Sorted by event date ascending (soonest first).
    /// </summary>
    public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
    {
        return await _context.Events
            .Where(e => e.EventDate >= DateTime.Today && e.Status == "Published")   // Future + published only
            .OrderBy(e => e.EventDate)                                              // Soonest first
            .Take(10)                                                               // Limit to 10
            .ToListAsync();
    }

    /// <summary>
    /// Searches events by name or location.
    /// Returns all events where the name OR location contains the search query.
    /// </summary>
    /// <param name="query">The search term</param>
    public async Task<IEnumerable<Event>> SearchEventsAsync(string query)
    {
        return await _context.Events
            .Where(e => e.Name.Contains(query) || e.Location.Contains(query))       // Name OR location match
            .ToListAsync();
    }
}