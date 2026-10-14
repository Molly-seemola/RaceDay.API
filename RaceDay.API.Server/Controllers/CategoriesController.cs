using Microsoft.AspNetCore.Mvc;
using RaceDay.API.DTOs;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Get all categories for an event (public)
    /// </summary>
    [HttpGet("events/{eventId}/categories")]
    public async Task<IActionResult> GetByEvent(int eventId)
    {
        var categories = await _categoryService.GetCategoriesByEventAsync(eventId);
        return Ok(categories);
    }

    /// <summary>
    /// Add a category to an event (Organiser only)
    /// </summary>
    [HttpPost("events/{eventId}/categories")]
    public async Task<IActionResult> Create(int eventId, [FromBody] CategoryDto dto)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        if (userId == null)
            return Unauthorized(new { message = "Not logged in" });

        if (role != "Organiser")
            return Forbid();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var category = await _categoryService.CreateCategoryAsync(eventId, userId.Value, dto);
            return CreatedAtAction(nameof(GetByEvent), new { eventId = eventId }, category);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Update a category (Organiser only)
    /// </summary>
    [HttpPut("categories/{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryDto dto)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        if (userId == null)
            return Unauthorized(new { message = "Not logged in" });

        if (role != "Organiser")
            return Forbid();

        var updated = await _categoryService.UpdateCategoryAsync(id, userId.Value, dto);
        if (updated == null)
            return NotFound(new { message = "Category not found or not owned by you" });

        return Ok(updated);
    }

    /// <summary>
    /// Delete a category (Organiser only)
    /// </summary>
    [HttpDelete("categories/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        if (userId == null)
            return Unauthorized(new { message = "Not logged in" });

        if (role != "Organiser")
            return Forbid();

        var deleted = await _categoryService.DeleteCategoryAsync(id, userId.Value);
        if (!deleted)
            return NotFound(new { message = "Category not found or not owned by you" });

        return NoContent();
    }
}