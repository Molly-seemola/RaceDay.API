using Microsoft.AspNetCore.Mvc;          
using RaceDay.API.DTOs;                  
using RaceDay.API.Services;              

namespace RaceDay.API.Controllers;       

/// <summary>
/// Controller that handles Authentication and User Profile operations.
/// Endpoints:
///   - POST /api/auth/register   → Register a new user (Organiser or Participant)
///   - POST /api/auth/login      → Login and create a session
///   - GET  /api/auth/me         → Get the current user's profile
///   - PUT  /api/auth/me         → Update the current user's profile
///   - POST /api/auth/logout     → Clear the current session
/// </summary>
[ApiController]                          // Enables automatic model validation and API-specific behaviors
[Route("api/[controller]")]              // Route becomes /api/auth  ([controller] = "Auth")
public class AuthController : ControllerBase
{
    // Dependency-injected service that contains the business logic for authentication
    private readonly IAuthService _authService;

    /// <summary>
    /// Constructor injects the IAuthService so we can use it in the endpoints below.
    /// </summary>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// POST /api/auth/register
    /// Register a new user account (Organiser or Participant).
    /// Public endpoint — no login required.
    /// </summary>
    /// <param name="dto">Registration data: email, password, first name, last name, role, etc.</param>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        // Validate the incoming DTO against any DataAnnotations
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Call the service to create the user and their role-specific profile
        var result = await _authService.RegisterAsync(dto);

        // If registration failed (e.g. email already exists), return 409 Conflict
        if (!result.Success)
            return Conflict(new { message = result.Message });

        // Return 201 Created with the newly registered user's basic details
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
    /// POST /api/auth/login
    /// Login with email and password. Creates a session on success.
    /// Public endpoint — no login required.
    /// </summary>
    /// <param name="dto">Login credentials: email and password</param>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        // Validate the incoming DTO against any DataAnnotations
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Call the service to verify the user's credentials
        var result = await _authService.LoginAsync(dto);

        // If login failed, return 401 Unauthorized
        if (!result.Success)
            return Unauthorized(new { message = result.Message });

        // Store the user's ID, role, and email in the session.
        // These values will be used to enforce role-based access on other endpoints.
        HttpContext.Session.SetInt32("UserId", result.User!.UserId);
        HttpContext.Session.SetString("UserRole", result.User.Role);
        HttpContext.Session.SetString("UserEmail", result.User.Email);

        // Return 200 OK with a success message and the user's details
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
    /// GET /api/auth/me
    /// Get the currently logged-in user's profile.
    /// Requires an active session.
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        // Get the user's ID from the session
        var userId = HttpContext.Session.GetInt32("UserId");

        // If no session exists, the user is not logged in → 401
        if (userId == null)
            return Unauthorized(new { message = "Not logged in" });

        // Fetch the user from the database
        var user = await _authService.GetUserByIdAsync(userId.Value);

        // If the user cannot be found → 404
        if (user == null)
            return NotFound(new { message = "User not found" });

        // Return 200 OK with the user's profile details
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
    /// PUT /api/auth/me
    /// Update the currently logged-in user's profile.
    /// Requires an active session.
    /// </summary>
    /// <param name="dto">Updated profile data (first name, last name, email, etc.)</param>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] RegisterDto dto)
    {
        // Get the user's ID from the session
        var userId = HttpContext.Session.GetInt32("UserId");

        // If no session exists, the user is not logged in → 401
        if (userId == null)
            return Unauthorized(new { message = "Not logged in" });

        // Call the service to update the user's profile
        var updated = await _authService.UpdateProfileAsync(userId.Value, dto);

        // If the user cannot be found → 404
        if (updated == null)
            return NotFound(new { message = "User not found" });

        // Return 200 OK with the updated profile details
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
    /// POST /api/auth/logout
    /// Logout the current user by clearing the session.
    /// Requires an active session.
    /// </summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // Clear all session data → user is now logged out
        HttpContext.Session.Clear();

        // Return 200 OK with a confirmation message
        return Ok(new { message = "Logged out successfully" });
    }
}