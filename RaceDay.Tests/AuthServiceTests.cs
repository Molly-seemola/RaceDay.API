using FluentAssertions;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;
using Xunit;

namespace RaceDay.Tests;

public class AuthServiceTests
{
    // ============================================
    // REGISTER TESTS
    // ============================================

    [Fact]
    public async Task Register_WithValidParticipant_ReturnsSuccess()
    {
        // ARRANGE
        var context = TestDbContextFactory.Create("Register_Valid_Participant");
        var service = new AuthService(context);
        var dto = new RegisterDto
        {
            Email = "newparticipant@test.com",
            Password = "Password123!",
            FirstName = "New",
            LastName = "User",
            Role = "Participant",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Male"
        };

        // ACT
        var result = await service.RegisterAsync(dto);

        // ASSERT
        result.Success.Should().BeTrue();
        result.User.Should().NotBeNull();
        result.User!.Email.Should().Be("newparticipant@test.com");
        result.User.Role.Should().Be("Participant");
    }

    [Fact]
    public async Task Register_WithValidOrganiser_ReturnsSuccess()
    {
        var context = TestDbContextFactory.Create("Register_Valid_Organiser");
        var service = new AuthService(context);
        var dto = new RegisterDto
        {
            Email = "neworganiser@test.com",
            Password = "Password123!",
            FirstName = "New",
            LastName = "Organiser",
            Role = "Organiser",
            CompanyName = "Test Events",
            ContactNumber = "+27123456789"
        };

        var result = await service.RegisterAsync(dto);

        result.Success.Should().BeTrue();
        result.User!.Role.Should().Be("Organiser");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsFail()
    {
        // ARRANGE
        var context = TestDbContextFactory.Create("Register_Duplicate_Email");
        var service = new AuthService(context);

        var dto1 = new RegisterDto
        {
            Email = "duplicate@test.com",
            Password = "Password123!",
            FirstName = "First",
            LastName = "User",
            Role = "Participant",
            DateOfBirth = DateTime.Now.AddYears(-20)
        };
        await service.RegisterAsync(dto1); // Register first user

        var dto2 = new RegisterDto
        {
            Email = "duplicate@test.com",  // Same email!
            Password = "Password123!",
            FirstName = "Second",
            LastName = "User",
            Role = "Participant",
            DateOfBirth = DateTime.Now.AddYears(-20)
        };

        // ACT
        var result = await service.RegisterAsync(dto2);

        // ASSERT
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already registered");
    }

    [Fact]
    public async Task Register_HashesPassword_NotPlainText()
    {
        var context = TestDbContextFactory.Create("Register_Hash_Password");
        var service = new AuthService(context);
        var dto = new RegisterDto
        {
            Email = "hash@test.com",
            Password = "MyPassword123!",
            FirstName = "Test",
            LastName = "Hash",
            Role = "Participant",
            DateOfBirth = DateTime.Now.AddYears(-20)
        };

        var result = await service.RegisterAsync(dto);

        var user = context.Users.First(u => u.Email == "hash@test.com");
        user.PasswordHash.Should().NotBe("MyPassword123!");   // Not stored as plain text
        user.PasswordHash.Should().StartWith("$2");           // BCrypt hash starts with $2
    }

    // ============================================
    // LOGIN TESTS
    // ============================================

    [Fact]
    public async Task Login_WithCorrectCredentials_ReturnsSuccess()
    {
        // ARRANGE
        var context = TestDbContextFactory.Create("Login_Correct");
        var service = new AuthService(context);

        // Register a user first
        await service.RegisterAsync(new RegisterDto
        {
            Email = "login@test.com",
            Password = "Password123!",
            FirstName = "Login",
            LastName = "User",
            Role = "Participant",
            DateOfBirth = DateTime.Now.AddYears(-20)
        });

        // ACT
        var result = await service.LoginAsync(new LoginDto
        {
            Email = "login@test.com",
            Password = "Password123!"
        });

        // ASSERT
        result.Success.Should().BeTrue();
        result.User.Should().NotBeNull();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsFail()
    {
        var context = TestDbContextFactory.Create("Login_Wrong_Password");
        var service = new AuthService(context);

        await service.RegisterAsync(new RegisterDto
        {
            Email = "wrongpass@test.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User",
            Role = "Participant",
            DateOfBirth = DateTime.Now.AddYears(-20)
        });

        var result = await service.LoginAsync(new LoginDto
        {
            Email = "wrongpass@test.com",
            Password = "WrongPassword!"
        });

        result.Success.Should().BeFalse();
        result.User.Should().BeNull();
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ReturnsFail()
    {
        var context = TestDbContextFactory.Create("Login_No_User");
        var service = new AuthService(context);

        var result = await service.LoginAsync(new LoginDto
        {
            Email = "doesnotexist@test.com",
            Password = "Password123!"
        });

        result.Success.Should().BeFalse();
    }
}

