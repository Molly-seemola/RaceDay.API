using Microsoft.AspNetCore.Mvc;
using RaceDay.API.DTOs;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new user account (Organiser or Participant)
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RegisterAsync(dto);

        if (!result.Success)
            return Conflict(new { message = result.Message });

        return CreatedAtAction(nameof(GetProfile), new { id = result.User!.UserId }, new
        {
            userId = result.User.UserId,
            email = result.User.Email,
            firstName = result.User.FirstName,
            lastName = result.User.LastName,
            role = result.User.Role,
            dateRegistered = result.User.DateRegistered
        });
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.LoginAsync(dto);

        if (!result.Success)
            return Unauthorized(new { message = result.Message });

        HttpContext.Session.SetInt32("UserId", result.User!.UserId);
        HttpContext.Session.SetString("UserRole", result.User.Role);
        HttpContext.Session.SetString("UserEmail", result.User.Email);

        return Ok(new
        {
            message = "Login successful",
            user = new
            {
                userId = result.User.UserId,
                email = result.User.Email,
                firstName = result.User.FirstName,
                lastName = result.User.LastName,
                role = result.User.Role
            }
        });
    }

    /// <summary>
    /// Get current user's profile
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Unauthorized(new { message = "Not logged in" });

        var user = await _authService.GetUserByIdAsync(userId.Value);
        if (user == null)
            return NotFound(new { message = "User not found" });

        return Ok(new
        {
            userId = user.UserId,
            email = user.Email,
            firstName = user.FirstName,
            lastName = user.LastName,
            role = user.Role,
            dateRegistered = user.DateRegistered,
            isActive = user.IsActive
        });
    }

    /// <summary>
    /// Update current user's profile
    /// </summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] RegisterDto dto)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return Unauthorized(new { message = "Not logged in" });

        var updated = await _authService.UpdateProfileAsync(userId.Value, dto);
        if (updated == null)
            return NotFound(new { message = "User not found" });

        return Ok(new
        {
            userId = updated.UserId,
            email = updated.Email,
            firstName = updated.FirstName,
            lastName = updated.LastName,
            role = updated.Role
        });
    }

    /// <summary>
    /// Logout the current user
    /// </summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new { message = "Logged out successfully" });
    }
}