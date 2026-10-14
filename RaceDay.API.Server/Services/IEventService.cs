using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Services;

public interface IEventService
{
    Task<IEnumerable<Event>> GetAllEventsAsync();
    Task<Event?> GetEventByIdAsync(int id);
    Task<Event> CreateEventAsync(int organiserId, EventDto dto);
    Task<Event?> UpdateEventAsync(int id, int organiserId, EventDto dto);
    Task<bool> DeleteEventAsync(int id, int organiserId);
    Task<IEnumerable<Event>> GetUpcomingEventsAsync();
    Task<IEnumerable<Event>> SearchEventsAsync(string query);
}