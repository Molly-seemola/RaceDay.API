using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Services;

public class AuthService : IAuthService
{
    private readonly RaceDayDbContext _context;

    public AuthService(RaceDayDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string Message, User? User)> RegisterAsync(RegisterDto dto)
    {
        // Check if email exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

        if (existingUser != null)
            return (false, "Email already registered", null);

        // Create user
        var user = new User
        {
            Email = dto.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Role = dto.Role,
            DateRegistered = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Create role-specific profile
        if (dto.Role == "Organiser")
        {
            var organiser = new Organiser
            {
                UserId = user.UserId,
                CompanyName = dto.CompanyName,
                ContactNumber = dto.ContactNumber ?? "N/A",
                IsVerified = false
            };
            _context.Organisers.Add(organiser);
        }
        else if (dto.Role == "Participant")
        {
            var participant = new Participant
            {
                UserId = user.UserId,
                DateOfBirth = dto.DateOfBirth ?? DateTime.Now.AddYears(-20),
                Gender = dto.Gender,
                EmergencyContact = dto.EmergencyContact,
                EmergencyPhone = dto.EmergencyPhone
            };
            _context.Participants.Add(participant);
        }

        await _context.SaveChangesAsync();

        return (true, "Registration successful", user);
    }

    public async Task<(bool Success, string Message, User? User)> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

        if (user == null)
            return (false, "Invalid email or password", null);

        if (!user.IsActive)
            return (false, "Account is disabled", null);

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return (false, "Invalid email or password", null);

        return (true, "Login successful", user);
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<User?> UpdateProfileAsync(int userId, RegisterDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        await _context.SaveChangesAsync();
        return user;
    }
}