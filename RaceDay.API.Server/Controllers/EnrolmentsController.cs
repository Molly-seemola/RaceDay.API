using Microsoft.AspNetCore.Mvc;
using RaceDay.API.DTOs;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api")]
public class EnrolmentsController : ControllerBase
{
    private readonly IEnrolmentService _enrolmentService;

    public EnrolmentsController(IEnrolmentService enrolmentService)
    {
        _enrolmentService = enrolmentService;
    }

    /// <summary>
    /// Get all enrolments (filtered by current user role)
    /// </summary>
    [HttpGet("enrolments")]
    public async Task<IActionResult> GetAll()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        if (userId == null)
            return Unauthorized(new { message = "Not logged in" });

        if (role == "Organiser")
        {
            var all = await _enrolmentService.GetEnrolmentsForOrganiserAsync(userId.Value);
            return Ok(all);
        }
        else
        {
            var mine = await _enrolmentService.GetEnrolmentsForParticipantAsync(userId.Value);
            return Ok(mine);
        }
    }

    /// <summary>
    /// Enrol in an event category (Participant only)
    /// </summary>
    [HttpPost("events/{eventId}/enrol")]
    public async Task<IActionResult> Enrol(int eventId, [FromBody] EnrolmentDto dto)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        if (userId == null)
            return Unauthorized(new { message = "Not logged in" });

        if (role != "Participant")
            return Forbid();

        try
        {
            var enrolment = await _enrolmentService.EnrolAsync(eventId, userId.Value, dto);
            return CreatedAtAction(nameof(GetAll), new { id = enrolment.EnrolmentId }, enrolment);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cancel an enrolment (Participant only)
    /// </summary>
    [HttpPut("enrolments/{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        if (userId == null)
            return Unauthorized(new { message = "Not logged in" });

        if (role != "Participant")
            return Forbid();

        var cancelled = await _enrolmentService.CancelEnrolmentAsync(id, userId.Value);
        if (cancelled == null)
            return NotFound(new { message = "Enrolment not found or not yours" });

        return Ok(new { message = "Enrolment cancelled", enrolmentId = id });
    }
}