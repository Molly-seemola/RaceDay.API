using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Services;

public interface IEnrolmentService
{
    Task<IEnumerable<object>> GetEnrolmentsForParticipantAsync(int userId);
    Task<IEnumerable<object>> GetEnrolmentsForOrganiserAsync(int userId);
    Task<Enrolment> EnrolAsync(int eventId, int userId, EnrolmentDto dto);
    Task<Enrolment?> CancelEnrolmentAsync(int enrolmentId, int userId);
}