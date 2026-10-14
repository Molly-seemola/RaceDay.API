using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Services;

public class EnrolmentService : IEnrolmentService
{
    private readonly RaceDayDbContext _context;

    public EnrolmentService(RaceDayDbContext context)
    {
        _context = context;
    }

    // Get all enrolments for a specific participant
    public async Task<IEnumerable<object>> GetEnrolmentsForParticipantAsync(int userId)
    {
        var participant = await _context.Participants
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (participant == null)
            return new List<object>();

        return await _context.Enrolments
            .Where(e => e.ParticipantId == participant.ParticipantId)
            .Include(e => e.Event)
            .Include(e => e.Category)
            .Include(e => e.Result)
            .Select(e => new
            {
                e.EnrolmentId,
                e.EventId,
                EventName = e.Event.Name,
                EventDate = e.Event.EventDate,
                e.CategoryId,
                CategoryName = e.Category.CategoryName,
                Distance = e.Category.Distance,
                e.Status,
                e.PaymentStatus,
                e.BibNumber,
                e.EnrolmentDate,
                FinishTime = e.Result != null ? e.Result.FinishTime : null,
                Position = e.Result != null ? e.Result.Position : null
            })
            .ToListAsync<object>();
    }

    // Get all enrolments for events owned by an organiser
    public async Task<IEnumerable<object>> GetEnrolmentsForOrganiserAsync(int userId)
    {
        var organiser = await _context.Organisers
            .FirstOrDefaultAsync(o => o.UserId == userId);

        if (organiser == null)
            return new List<object>();

        return await _context.Enrolments
            .Where(e => e.Event.OrganiserId == organiser.OrganiserId)
            .Include(e => e.Event)
            .Include(e => e.Category)
            .Include(e => e.Participant).ThenInclude(p => p.User)
            .Select(e => new
            {
                e.EnrolmentId,
                e.EventId,
                EventName = e.Event.Name,
                e.CategoryId,
                CategoryName = e.Category.CategoryName,
                ParticipantId = e.ParticipantId,
                ParticipantName = e.Participant.User.FirstName + " " + e.Participant.User.LastName,
                ParticipantEmail = e.Participant.User.Email,
                e.Status,
                e.PaymentStatus,
                e.BibNumber,
                e.EnrolmentDate
            })
            .ToListAsync<object>();
    }

    // Enrol a participant in an event category
    public async Task<Enrolment> EnrolAsync(int eventId, int userId, EnrolmentDto dto)
    {
        var participant = await _context.Participants
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (participant == null)
            throw new InvalidOperationException("Participant profile not found");

        // Check if event exists
        var eventEntity = await _context.Events
            .FirstOrDefaultAsync(e => e.EventId == eventId);

        if (eventEntity == null)
            throw new InvalidOperationException("Event not found");

        // Check if category exists and belongs to the event
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == dto.CategoryId && c.EventId == eventId);

        if (category == null)
            throw new InvalidOperationException("Category not found for this event");

        // Check if already enrolled
        var existing = await _context.Enrolments
            .FirstOrDefaultAsync(e => e.EventId == eventId
                                   && e.ParticipantId == participant.ParticipantId
                                   && e.CategoryId == dto.CategoryId);

        if (existing != null)
            throw new InvalidOperationException("You are already enrolled in this category");

        // Generate a bib number
        var maxBib = await _context.Enrolments
            .Where(e => e.BibNumber != null)
            .MaxAsync(e => (int?)e.BibNumber) ?? 1000;

        var enrolment = new Enrolment
        {
            EventId = eventId,
            ParticipantId = participant.ParticipantId,
            CategoryId = dto.CategoryId,
            EnrolmentDate = DateTime.UtcNow,
            Status = "Pending",
            PaymentStatus = dto.PaymentStatus ?? "Unpaid",
            BibNumber = maxBib + 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Enrolments.Add(enrolment);

        // Update category count
        category.CurrentCount += 1;

        await _context.SaveChangesAsync();
        return enrolment;
    }

    // Cancel an enrolment
    public async Task<Enrolment?> CancelEnrolmentAsync(int enrolmentId, int userId)
    {
        var participant = await _context.Participants
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (participant == null)
            return null;

        var enrolment = await _context.Enrolments
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.EnrolmentId == enrolmentId
                                   && e.ParticipantId == participant.ParticipantId);

        if (enrolment == null)
            return null;

        enrolment.Status = "Cancelled";
        enrolment.UpdatedAt = DateTime.UtcNow;

        // Decrease category count
        if (enrolment.Category != null && enrolment.Category.CurrentCount > 0)
        {
            enrolment.Category.CurrentCount -= 1;
        }

        await _context.SaveChangesAsync();
        return enrolment;
    }
}