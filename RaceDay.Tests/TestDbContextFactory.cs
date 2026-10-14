using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using FluentAssertions;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Services;
using Xunit;

namespace RaceDay.Tests;

public static class TestDbContextFactory
{
    public static RaceDayDbContext Create(string databaseName = "TestDb")
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new RaceDayDbContext(options);
    }
}