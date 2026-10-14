using Microsoft.AspNetCore.Mvc;
using RaceDay.API.DTOs;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api")]
public class ResultsController : ControllerBase
{
    private readonly IResultService _resultService;

    public ResultsController(IResultService resultService)
    {
        _resultService = resultService;
    }

    /// <summary>
    /// Get all results (filtered by event or participant)
    /// </summary>
    [HttpGet("results")]
    public async Task<IActionResult> GetAll([FromQuery] int? eventId, [FromQuery] int? participantId)
    {
        var results = await _resultService.GetResultsAsync(eventId, participantId);
        return Ok(results);
    }

    /// <summary>
    /// Submit a participant result (Organiser only)
    /// </summary>
    [HttpPost("results")]
    public async Task<IActionResult> Submit([FromBody] ResultDto dto)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        if (userId == null)
            return Unauthorized(new { message = "Not logged in" });

        if (role != "Organiser")
            return Forbid();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _resultService.CreateResultAsync(userId.Value, dto);
        return CreatedAtAction(nameof(GetAll), new { id = result.ResultId }, result);
    }

    /// <summary>
    /// Update a result (Organiser only)
    /// </summary>
    [HttpPut("results/{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ResultDto dto)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        if (userId == null)
            return Unauthorized(new { message = "Not logged in" });

        if (role != "Organiser")
            return Forbid();

        var updated = await _resultService.UpdateResultAsync(id, userId.Value, dto);
        if (updated == null)
            return NotFound(new { message = "Result not found" });

        return Ok(updated);
    }

    /// <summary>
    /// Get a participant's result history (Participant only — own results)
    /// </summary>
    [HttpGet("participants/{id}/results")]
    public async Task<IActionResult> GetParticipantResults(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        if (userId == null)
            return Unauthorized(new { message = "Not logged in" });

        if (role != "Participant" || userId.Value != id)
            return Forbid();

        var results = await _resultService.GetResultsAsync(null, id);
        return Ok(results);
    }
}