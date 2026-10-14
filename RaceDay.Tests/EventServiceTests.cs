using FluentAssertions;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;
using Xunit;

namespace RaceDay.Tests;

public class EventServiceTests
{
    // Helper to create a test organiser
    private async Task<(int userId, int organiserId)> CreateTestOrganiser(RaceDay.API.Data.RaceDayDbContext context)
    {
        var user = new User
        {
            Email = "organiser@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            FirstName = "Test",
            LastName = "Organiser",
            Role = "Organiser"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var organiser = new Organiser
        {
            UserId = user.UserId,
            CompanyName = "Test Events",
            ContactNumber = "+27123456789"
        };
        context.Organisers.Add(organiser);
        await context.SaveChangesAsync();

        return (user.UserId, organiser.OrganiserId);
    }

    [Fact]
    public async Task CreateEvent_AsOrganiser_ReturnsSuccess()
    {
        // ARRANGE
        var context = TestDbContextFactory.Create("Create_Event_Success");
        var (userId, _) = await CreateTestOrganiser(context);
        var service = new EventService(context);

        var dto = new EventDto
        {
            Name = "Test Marathon",
            Description = "A test event",
            EventDate = DateTime.Now.AddDays(30),
            StartTime = new TimeSpan(6, 0, 0),
            Location = "Test Location",
            EventType = "Running",
            Status = "Published",
            MaxParticipants = 500,
            Distance = 21.1m
        };

        // ACT
        var result = await service.CreateEventAsync(userId, dto);

        // ASSERT
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Marathon");
        result.EventType.Should().Be("Running");
    }

    [Fact]
    public async Task CreateEvent_AsNonOrganiser_ThrowsException()
    {
        // ARRANGE
        var context = TestDbContextFactory.Create("Create_Event_Not_Organiser");
        var service = new EventService(context);

        var dto = new EventDto
        {
            Name = "Test",
            EventDate = DateTime.Now.AddDays(30),
            StartTime = new TimeSpan(6, 0, 0),
            Location = "Test",
            EventType = "Running",
            MaxParticipants = 100
        };

        // ACT & ASSERT
        // userId = 9999 doesn't exist as an organiser
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await service.CreateEventAsync(9999, dto));
    }

    [Fact]
    public async Task GetAllEvents_ReturnsListOfEvents()
    {
        // ARRANGE
        var context = TestDbContextFactory.Create("Get_All_Events");
        var (userId, _) = await CreateTestOrganiser(context);
        var service = new EventService(context);

        // Create 2 events
        await service.CreateEventAsync(userId, new EventDto
        {
            Name = "Event 1",
            EventDate = DateTime.Now.AddDays(10),
            StartTime = new TimeSpan(6, 0, 0),
            Location = "Location 1",
            EventType = "Running",
            MaxParticipants = 100
        });

        await service.CreateEventAsync(userId, new EventDto
        {
            Name = "Event 2",
            EventDate = DateTime.Now.AddDays(20),
            StartTime = new TimeSpan(7, 0, 0),
            Location = "Location 2",
            EventType = "Cycling",
            MaxParticipants = 200
        });

        // ACT
        var events = await service.GetAllEventsAsync();

        // ASSERT
        events.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeleteEvent_WithValidId_ReturnsTrue()
    {
        // ARRANGE
        var context = TestDbContextFactory.Create("Delete_Event_Valid");
        var (userId, _) = await CreateTestOrganiser(context);
        var service = new EventService(context);

        var created = await service.CreateEventAsync(userId, new EventDto
        {
            Name = "To Delete",
            EventDate = DateTime.Now.AddDays(10),
            StartTime = new TimeSpan(6, 0, 0),
            Location = "Location",
            EventType = "Running",
            MaxParticipants = 100
        });

        // ACT
        var result = await service.DeleteEventAsync(created.EventId, userId);

        // ASSERT
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteEvent_WithWrongOrganiser_ReturnsFalse()
    {
        // ARRANGE
        var context = TestDbContextFactory.Create("Delete_Event_Wrong_Owner");
        var (userId, _) = await CreateTestOrganiser(context);
        var service = new EventService(context);

        var created = await service.CreateEventAsync(userId, new EventDto
        {
            Name = "Event",
            EventDate = DateTime.Now.AddDays(10),
            StartTime = new TimeSpan(6, 0, 0),
            Location = "Location",
            EventType = "Running",
            MaxParticipants = 100
        });

        // ACT - Delete with a DIFFERENT userId (different organiser)
        var result = await service.DeleteEventAsync(created.EventId, 9999);

        // ASSERT
        result.Should().BeFalse();
    }
}