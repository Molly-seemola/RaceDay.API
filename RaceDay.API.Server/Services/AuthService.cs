using Microsoft.EntityFrameworkCore;      
using RaceDay.API.Data;                   
using RaceDay.API.DTOs;                    
using RaceDay.API.Models;                  

namespace RaceDay.API.Services;            

/// Passwords are always hashed using BCrypt — plain-text passwords are never stored.
/// </summary>
public class AuthService : IAuthService
{
    // Database context used for all database operations
    private readonly RaceDayDbContext _context;

    /// <summary>
    /// Constructor injects the database context.
    /// </summary>
    public AuthService(RaceDayDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Registers a new user account.
    ///   - Checks for duplicate emails
    ///   - Hashes the password using BCrypt
    ///   - Creates the base User record
    ///   - Creates the role-specific profile (Organiser or Participant)
    /// </summary>
    /// <param name="dto">The registration data from the client</param>
    /// <returns>A tuple with success status, message, and the created User (if successful)</returns>
    public async Task<(bool Success, string Message, User? User)> RegisterAsync(RegisterDto dto)
    {
        // Check if email exists
        // Case-insensitive check to prevent duplicate emails with different casing
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

        // If a user with this email already exists, return failure
        if (existingUser != null)
            return (false, "Email already registered", null);

        // Create user
        // Password is hashed using BCrypt before saving — never stored as plain text
        var user = new User
        {
            Email = dto.Email.ToLower(),                                    // Store as lowercase for consistency
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),    // Hashed password
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Role = dto.Role,                                                // "Organiser" or "Participant"
            DateRegistered = DateTime.UtcNow,
            IsActive = true
        };

        // Save the User first so it gets a UserId (needed for the profile below)
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Create role-specific profile
        // If the user is an Organiser, create an Organiser profile
        if (dto.Role == "Organiser")
        {
            var organiser = new Organiser
            {
                UserId = user.UserId,
                CompanyName = dto.CompanyName,
                ContactNumber = dto.ContactNumber ?? "N/A",                 // Default to "N/A" if not provided
                IsVerified = false                                          // New organisers start unverified
            };
            _context.Organisers.Add(organiser);
        }
        // If the user is a Participant, create a Participant profile
        else if (dto.Role == "Participant")
        {
            var participant = new Participant
            {
                UserId = user.UserId,
                DateOfBirth = dto.DateOfBirth ?? DateTime.Now.AddYears(-20),// Default to ~20 years old if not provided
                Gender = dto.Gender,
                EmergencyContact = dto.EmergencyContact,
                EmergencyPhone = dto.EmergencyPhone
            };
            _context.Participants.Add(participant);
        }

        // Save the role-specific profile
        await _context.SaveChangesAsync();

        // Return success with the created user
        return (true, "Registration successful", user);
    }

    /// <summary>
    /// Logs in a user by verifying their email and password.
    ///   - Looks up the user by email
    ///   - Checks if the account is active
    ///   - Verifies the password against the stored BCrypt hash
    /// </summary>
    /// <param name="dto">Login credentials (email + password)</param>
    /// <returns>A tuple with success status, message, and the User (if successful)</returns>
    public async Task<(bool Success, string Message, User? User)> LoginAsync(LoginDto dto)
    {
        // Look up user by email (case-insensitive)
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

        // If no user found → fail with generic message (prevents email enumeration attacks)
        if (user == null)
            return (false, "Invalid email or password", null);

        // If the account is disabled → fail
        if (!user.IsActive)
            return (false, "Account is disabled", null);

        // Verify the provided password against the stored BCrypt hash
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return (false, "Invalid email or password", null);

        // Everything checks out → login successful
        return (true, "Login successful", user);
    }

    /// <summary>
    /// Retrieves a user by their UserId.
    /// </summary>
    /// <param name="userId">The user's unique ID</param>
    /// <returns>The User entity or null if not found</returns>
    public async Task<User?> GetUserByIdAsync(int userId)
    {
        // FindAsync is efficient for primary key lookups
        return await _context.Users.FindAsync(userId);
    }

    /// <summary>
    /// Updates the current user's basic profile information (name only).
    /// </summary>
    /// <param name="userId">The user's unique ID</param>
    /// <param name="dto">The updated profile data</param>
    /// <returns>The updated User entity or null if not found</returns>
    public async Task<User?> UpdateProfileAsync(int userId, RegisterDto dto)
    {
        // Look up the user
        var user = await _context.Users.FindAsync(userId);

        // If no user found, return null
        if (user == null) return null;

        // Update allowed fields
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;

        // Save changes
        await _context.SaveChangesAsync();

        // Return the updated user
        return user;
    }
}