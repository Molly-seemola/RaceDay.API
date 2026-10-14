using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Services;

public interface IAuthService
{
    Task<(bool Success, string Message, User? User)> RegisterAsync(RegisterDto dto);
    Task<(bool Success, string Message, User? User)> LoginAsync(LoginDto dto);
    Task<User?> GetUserByIdAsync(int userId);
    Task<User?> UpdateProfileAsync(int userId, RegisterDto dto);
}