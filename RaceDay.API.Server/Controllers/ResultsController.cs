using Microsoft.AspNetCore.Mvc;          
using RaceDay.API.DTOs;                 
using RaceDay.API.Services;              

namespace RaceDay.API.Controllers;       // Groups this controller under the RaceDay.API.Controllers namespace

/// <summary>
/// Controller that handles all Result-related API operations.
/// Supports two roles:
///   - Organisers: can submit and update results for their events
///   - Participants: can view their own result history
/// </summary>
[ApiController]                          // Enables automatic model validation and API-specific behaviors
[Route("api")]                           // Base route for this controller, e.g. /api/results
public class ResultsController : ControllerBase
{
    // Dependency-injected service that contains the business logic for results
    private readonly IResultService _resultService;

    /// <summary>
    /// Constructor injects the IResultService so we can use it in the endpoints below.
    /// </summary>
    public ResultsController(IResultService resultService)
    {
        _resultService = resultService;
    }

    /// <summary>
    /// GET /api/results
    /// Get all results (filtered by event or participant).
    /// Public endpoint — no login required.
    /// </summary>
    /// <param name="eventId">Optional: filter results by event ID</param>
    /// <param name="participantId">Optional: filter results by participant ID</param>
    [HttpGet("results")]
    public async Task<IActionResult> GetAll([FromQuery] int? eventId, [FromQuery] int? participantId)
    {
        // Call the service to fetch the filtered list of results
        var results = await _resultService.GetResultsAsync(eventId, participantId);

        // Return 200 OK with the list of results
        return Ok(results);
    }

    /// <summary>
    /// POST /api/results
    /// Submit a participant result (Organiser only).
    /// </summary>
    /// <param name="dto">The result data (enrolmentId, finish time, position, etc.)</param>
    [HttpPost("results")]
    public async Task<IActionResult> Submit([FromBody] ResultDto dto)
    {
        // Get the current user's ID and role from session
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        // If no session exists, the user is not logged in → 401
        if (userId == null)
            return Unauthorized(new { message = "Not logged in" });

        // Only Organisers are allowed to submit results → 403 for anyone else
        if (role != "Organiser")
            return Forbid();

        // Validate the incoming DTO against any DataAnnotations
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Call the service to persist the new result
        var result = await _resultService.CreateResultAsync(userId.Value, dto);

        // Return 201 Created with the newly created result
        return CreatedAtAction(nameof(GetAll), new { id = result.ResultId }, result);
    }

    /// <summary>
    /// PUT /api/results/{id}
    /// Update an existing result (Organiser only).
    /// </summary>
    /// <param name="id">The ID of the result to update</param>
    /// <param name="dto">The updated result data</param>
    [HttpPut("results/{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ResultDto dto)
    {
        // Get the current user's ID and role from session
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        // Reject unauthenticated requests → 401
        if (userId == null)
            return Unauthorized(new { message = "Not logged in" });

        // Only Organisers can update results → 403 for Participants
        if (role != "Organiser")
            return Forbid();

        // Attempt the update in the service layer
        var updated = await _resultService.UpdateResultAsync(id, userId.Value, dto);

        // If the result doesn't exist OR isn't owned by this organiser → 404
        if (updated == null)
            return NotFound(new { message = "Result not found" });

        // Return the updated result
        return Ok(updated);
    }

    /// <summary>
    /// GET /api/participants/{id}/results
    /// Get a participant's result history.
    /// Access is restricted to the participant themselves (own results only).
    /// </summary>
    /// <param name="id">The participant ID whose results are being requested</param>
    [HttpGet("participants/{id}/results")]
    public async Task<IActionResult> GetParticipantResults(int id)
    {
        // Get the current user's ID and role from session
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        // Reject unauthenticated requests → 401
        if (userId == null)
            return Unauthorized(new { message = "Not logged in" });

        // Only the participant themselves can view their results:
        //   - Role must be "Participant"
        //   - The logged-in user ID must match the requested {id}
        if (role != "Participant" || userId.Value != id)
            return Forbid();

        // Fetch only this participant's results
        var results = await _resultService.GetResultsAsync(null, id);

        // Return 200 OK with the list
        return Ok(results);
    }
}