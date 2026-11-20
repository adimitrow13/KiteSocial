using KiteSocial.API.DTOs.Sessions;
using KiteSocial.API.Models.Entities;
using KiteSocial.API.Services;
using KiteSocial.API.Services.Interfaces;
using KiteSocial.Tests.Common;
using Xunit;

namespace KiteSocial.Tests.Services;

public class SessionServiceTests
{
    [Fact]
    public async Task GetUserStatsAsync_ShouldCalculateTotalsAndMaxJump()
    {
        // 1. Arrange
        using var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var spot = new Spot { Id = Guid.NewGuid(), Name = "Gradina", Country = "Bulgaria" };
        db.Spots.Add(spot);

        db.KiteSessions.AddRange(
            new KiteSession { UserId = userId, SpotId = spot.Id, Spot = spot, DurationMinutes = 60, MaxJumpHeightMeters = 7.5 },
            new KiteSession { UserId = userId, SpotId = spot.Id, Spot = spot, DurationMinutes = 90, MaxJumpHeightMeters = 12.0 }
        );
        await db.SaveChangesAsync();

        var service = new SessionService(db);

        // 2. Act
        var stats = await service.GetUserStatsAsync(userId);

        // 3. Assert
        Assert.Equal(2, stats.TotalSessions);
        Assert.Equal(150, stats.TotalMinutesOnWater);
        Assert.Equal(12.0, stats.MaxJumpHeightRecord);
        Assert.Equal("Gradina", stats.FavoriteSpotName);
    }

    [Fact]
    public async Task GetUserStatsAsync_ShouldReturnZeroes_WhenUserHasNoSessions()
    {
        // 1. Arrange
        using var db = TestDbContextFactory.Create();
        var service = new SessionService(db);

        // 2. Act
        var stats = await service.GetUserStatsAsync(Guid.NewGuid());

        // 3. Assert
        Assert.Equal(0, stats.TotalSessions);
        Assert.Equal(0, stats.TotalMinutesOnWater);
        Assert.Null(stats.MaxJumpHeightRecord);
        Assert.Null(stats.FavoriteSpotName);
    }

    [Fact]
    public async Task DeleteSessionAsync_ShouldReturnNotFound_WhenSessionDoesNotExist()
    {
        // 1. Arrange
        using var db = TestDbContextFactory.Create();
        var service = new SessionService(db);

        // 2. Act
        var result = await service.DeleteSessionAsync(Guid.NewGuid(), Guid.NewGuid());

        // 3. Assert
        Assert.Equal(SessionDeleteResult.NotFound, result);
    }

    [Fact]
    public async Task DeleteSessionAsync_ShouldReturnForbidden_WhenUserIsNotOwner()
    {
        // 1. Arrange
        using var db = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        var strangerId = Guid.NewGuid();
        var session = new KiteSession { Id = Guid.NewGuid(), UserId = ownerId };
        db.KiteSessions.Add(session);
        await db.SaveChangesAsync();

        var service = new SessionService(db);

        // 2. Act
        var result = await service.DeleteSessionAsync(session.Id, strangerId);

        // 3. Assert
        Assert.Equal(SessionDeleteResult.Forbidden, result);
    }

    [Fact]
    public async Task DeleteSessionAsync_ShouldReturnSuccess_WhenUserIsOwner()
    {
        // 1. Arrange
        using var db = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        var session = new KiteSession { Id = Guid.NewGuid(), UserId = ownerId };
        db.KiteSessions.Add(session);
        await db.SaveChangesAsync();

        var service = new SessionService(db);

        // 2. Act
        var result = await service.DeleteSessionAsync(session.Id, ownerId);

        // 3. Assert
        Assert.Equal(SessionDeleteResult.Success, result);
        var inDb = await db.KiteSessions.FindAsync(session.Id);
        Assert.Null(inDb);
    }

    [Fact]
    public async Task CreateSessionAsync_ShouldReturnNull_WhenSpotDoesNotExist()
    {
        // 1. Arrange
        using var db = TestDbContextFactory.Create();
        var service = new SessionService(db);
        var dto = new CreateKiteSessionDto { SpotId = Guid.NewGuid(), DurationMinutes = 60 };

        // 2. Act
        var result = await service.CreateSessionAsync(dto, Guid.NewGuid());

        // 3. Assert
        Assert.Null(result);
    }
}
