using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Services;

public class EventService : IEventService
{
    private readonly RaceDayDbContext _context;

    public EventService(RaceDayDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Event>> GetAllEventsAsync()
    {
        return await _context.Events
            .Include(e => e.Organiser).ThenInclude(o => o.User)
            .OrderByDescending(e => e.EventDate)
            .ToListAsync();
    }

    public async Task<Event?> GetEventByIdAsync(int id)
    {
        return await _context.Events
            .Include(e => e.Organiser).ThenInclude(o => o.User)
            .Include(e => e.Categories)
            .FirstOrDefaultAsync(e => e.EventId == id);
    }

    public async Task<Event> CreateEventAsync(int organiserId, EventDto dto)
    {
        var organiser = await _context.Organisers
            .FirstOrDefaultAsync(o => o.UserId == organiserId);

        if (organiser == null)
            throw new UnauthorizedAccessException("Organiser profile not found");

        var eventEntity = new Event
        {
            OrganiserId = organiser.OrganiserId,
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

        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();
        return eventEntity;
    }

    public async Task<Event?> UpdateEventAsync(int id, int organiserId, EventDto dto)
    {
        var eventEntity = await _context.Events
            .Include(e => e.Organiser)
            .FirstOrDefaultAsync(e => e.EventId == id);

        if (eventEntity == null) return null;
        if (eventEntity.Organiser.UserId != organiserId) return null;

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
        eventEntity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return eventEntity;
    }

    public async Task<bool> DeleteEventAsync(int id, int organiserId)
    {
        var eventEntity = await _context.Events
            .Include(e => e.Organiser)
            .FirstOrDefaultAsync(e => e.EventId == id);

        if (eventEntity == null) return false;
        if (eventEntity.Organiser.UserId != organiserId) return false;

        _context.Events.Remove(eventEntity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
    {
        return await _context.Events
            .Where(e => e.EventDate >= DateTime.Today && e.Status == "Published")
            .OrderBy(e => e.EventDate)
            .Take(10)
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> SearchEventsAsync(string query)
    {
        return await _context.Events
            .Where(e => e.Name.Contains(query) || e.Location.Contains(query))
            .ToListAsync();
    }
}