using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Services;

public class ResultService : IResultService
{
    private readonly RaceDayDbContext _context;

    public ResultService(RaceDayDbContext context)
    {
        _context = context;
    }

    // Get all results, optionally filtered by event or participant
    public async Task<IEnumerable<object>> GetResultsAsync(int? eventId, int? participantId)
    {
        var query = _context.Results
            .Include(r => r.Enrolment)
                .ThenInclude(e => e.Event)
            .Include(r => r.Enrolment)
                .ThenInclude(e => e.Category)
            .Include(r => r.Enrolment)
                .ThenInclude(e => e.Participant)
                    .ThenInclude(p => p.User)
            .AsQueryable();

        if (eventId.HasValue)
            query = query.Where(r => r.Enrolment.EventId == eventId.Value);

        if (participantId.HasValue)
            query = query.Where(r => r.Enrolment.ParticipantId == participantId.Value);

        return await query
            .Select(r => new
            {
                r.ResultId,
                r.EnrolmentId,
                EventId = r.Enrolment.EventId,
                EventName = r.Enrolment.Event.Name,
                CategoryName = r.Enrolment.Category.CategoryName,
                ParticipantId = r.Enrolment.ParticipantId,
                ParticipantName = r.Enrolment.Participant.User.FirstName + " "
                                + r.Enrolment.Participant.User.LastName,
                r.FinishTime,
                r.Position,
                r.OverallPosition,
                r.AgeCategoryPosition,
                r.Status
            })
            .ToListAsync<object>();
    }

    // Create a new result (Organiser only)
    public async Task<Result> CreateResultAsync(int organiserUserId, ResultDto dto)
    {
        var enrolment = await _context.Enrolments
            .Include(e => e.Event)
                .ThenInclude(ev => ev.Organiser)
            .FirstOrDefaultAsync(e => e.EnrolmentId == dto.EnrolmentId);

        if (enrolment == null)
            throw new InvalidOperationException("Enrolment not found");

        if (enrolment.Event.Organiser.UserId != organiserUserId)
            throw new UnauthorizedAccessException("You can only add results to your own events");

        // Check if a result already exists for this enrolment
        var existing = await _context.Results
            .FirstOrDefaultAsync(r => r.EnrolmentId == dto.EnrolmentId);

        if (existing != null)
            throw new InvalidOperationException("A result already exists for this enrolment");

        var result = new Result
        {
            EnrolmentId = dto.EnrolmentId,
            FinishTime = dto.FinishTime,
            Position = dto.Position,
            OverallPosition = dto.OverallPosition,
            AgeCategoryPosition = dto.AgeCategoryPosition,
            Status = dto.Status ?? "Confirmed",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Results.Add(result);
        await _context.SaveChangesAsync();
        return result;
    }

    // Update an existing result (Organiser only)
    public async Task<Result?> UpdateResultAsync(int resultId, int organiserUserId, ResultDto dto)
    {
        var result = await _context.Results
            .Include(r => r.Enrolment)
                .ThenInclude(e => e.Event)
                    .ThenInclude(ev => ev.Organiser)
            .FirstOrDefaultAsync(r => r.ResultId == resultId);

        if (result == null)
            return null;

        if (result.Enrolment.Event.Organiser.UserId != organiserUserId)
            return null;

        result.FinishTime = dto.FinishTime;
        result.Position = dto.Position;
        result.OverallPosition = dto.OverallPosition;
        result.AgeCategoryPosition = dto.AgeCategoryPosition;
        result.Status = dto.Status ?? result.Status;
        result.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return result;
    }
}