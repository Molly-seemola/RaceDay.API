using FluentAssertions;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;
using Xunit;


namespace RaceDay.Tests;

public class EnrolmentServiceTests
{
    private async Task<(int participantUserId, int participantId)> CreateTestParticipant(RaceDayDbContext context)
    {
        var user = new User
        {
            Email = "participant@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            FirstName = "Test",
            LastName = "Participant",
            Role = "Participant"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var participant = new Participant
        {
            UserId = user.UserId,
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Male"
        };
        context.Participants.Add(participant);
        await context.SaveChangesAsync();

        return (user.UserId, participant.ParticipantId);
    }

    private async Task<int> CreateTestEventWithCategory(RaceDayDbContext context)
    {
        // Create organiser
        var organiserUser = new User
        {
            Email = "organiser@test.com",
            PasswordHash = "hash",
            FirstName = "Org",
            LastName = "User",
            Role = "Organiser"
        };
        context.Users.Add(organiserUser);
        await context.SaveChangesAsync();

        var organiser = new Organiser
        {
            UserId = organiserUser.UserId,
            ContactNumber = "+27123456789"
        };
        context.Organisers.Add(organiser);
        await context.SaveChangesAsync();

        // Create event
        var eventEntity = new Event
        {
            OrganiserId = organiser.OrganiserId,
            Name = "Test Event",
            EventDate = DateTime.Now.AddDays(10),
            StartTime = new TimeSpan(6, 0, 0),
            Location = "Test Location",
            EventType = "Running",
            Status = "Published",
            MaxParticipants = 100
        };
        context.Events.Add(eventEntity);
        await context.SaveChangesAsync();

        // Create category
        var category = new Category
        {
            EventId = eventEntity.EventId,
            CategoryName = "10km",
            Distance = 10,
            EntryFee = 100,
            MaxParticipants = 50
        };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        return eventEntity.EventId;
    }

    [Fact]
    public async Task Enrol_AsParticipant_ReturnsSuccess()
    {
        // ARRANGE
        var context = TestDbContextFactory.Create("Enrol_Success");
        var (userId, _) = await CreateTestParticipant(context);
        var eventId = await CreateTestEventWithCategory(context);
        var categoryId = context.Categories.First().CategoryId;
        var service = new EnrolmentService(context);

        var dto = new EnrolmentDto
        {
            CategoryId = categoryId,
            PaymentStatus = "Paid"
        };

        // ACT
        var result = await service.EnrolAsync(eventId, userId, dto);

        // ASSERT
        result.Should().NotBeNull();
        result.EventId.Should().Be(eventId);
        result.CategoryId.Should().Be(categoryId);
        result.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task Enrol_Twice_ThrowsException()
    {
        // ARRANGE
        var context = TestDbContextFactory.Create("Enrol_Duplicate");
        var (userId, _) = await CreateTestParticipant(context);
        var eventId = await CreateTestEventWithCategory(context);
        var categoryId = context.Categories.First().CategoryId;
        var service = new EnrolmentService(context);

        var dto = new EnrolmentDto { CategoryId = categoryId };
        await service.EnrolAsync(eventId, userId, dto); // First enrol

        // ACT & ASSERT
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.EnrolAsync(eventId, userId, dto)); // Second enrol
    }

    [Fact]
    public async Task CancelEnrolment_WithValidId_ReturnsSuccess()
    {
        // ARRANGE
        var context = TestDbContextFactory.Create("Cancel_Valid");
        var (userId, _) = await CreateTestParticipant(context);
        var eventId = await CreateTestEventWithCategory(context);
        var categoryId = context.Categories.First().CategoryId;
        var service = new EnrolmentService(context);

        var enrolment = await service.EnrolAsync(eventId, userId, new EnrolmentDto
        {
            CategoryId = categoryId
        });

        // ACT
        var result = await service.CancelEnrolmentAsync(enrolment.EnrolmentId, userId);

        // ASSERT
        result.Should().NotBeNull();
        result!.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task GetEnrolmentsForParticipant_ReturnsOnlyTheirEnrolments()
    {
        // ARRANGE
        var context = TestDbContextFactory.Create("Get_Participant_Enrolments");
        var (userId, _) = await CreateTestParticipant(context);
        var eventId = await CreateTestEventWithCategory(context);
        var categoryId = context.Categories.First().CategoryId;
        var service = new EnrolmentService(context);

        await service.EnrolAsync(eventId, userId, new EnrolmentDto { CategoryId = categoryId });

        // ACT
        var enrolments = await service.GetEnrolmentsForParticipantAsync(userId);

        // ASSERT
        enrolments.Should().HaveCount(1);
    }
}