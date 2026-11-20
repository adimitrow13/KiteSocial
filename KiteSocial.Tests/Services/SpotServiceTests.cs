using KiteSocial.API.DTOs.Spots;
using KiteSocial.API.Models.Entities;
using KiteSocial.API.Services;
using KiteSocial.Tests.Common;
using Xunit;

namespace KiteSocial.Tests.Services;

public class SpotServiceTests
{
    [Fact]
    public async Task GetAllSpotsAsync_ShouldFilterBySearchAndCountry()
    {
        // 1. Arrange
        using var db = TestDbContextFactory.Create();
        db.Spots.AddRange(
            new Spot { Id = Guid.NewGuid(), Name = "Gradina Beach", Country = "Bulgaria", WaterType = WaterType.Chop },
            new Spot { Id = Guid.NewGuid(), Name = "Dakhla Lagoon", Country = "Morocco", WaterType = WaterType.Flat },
            new Spot { Id = Guid.NewGuid(), Name = "Lozenets", Country = "Bulgaria", WaterType = WaterType.Waves }
        );
        await db.SaveChangesAsync();

        var service = new SpotService(db);

        // 2. Act
        var bulgarianSpots = await service.GetAllSpotsAsync(null, "Bulgaria", null);
        var searchResults = await service.GetAllSpotsAsync("Lagoon", null, null);

        // 3. Assert
        Assert.Equal(2, bulgarianSpots.Count());
        Assert.Single(searchResults);
        Assert.Equal("Dakhla Lagoon", searchResults.First().Name);
    }

    [Fact]
    public async Task GetSpotByIdAsync_ShouldReturnSpot_WhenExists()
    {
        // 1. Arrange
        using var db = TestDbContextFactory.Create();
        var spotId = Guid.NewGuid();
        db.Spots.Add(new Spot { Id = spotId, Name = "Gokova Bay", Country = "Turkey", WaterType = WaterType.Flat });
        await db.SaveChangesAsync();

        var service = new SpotService(db);

        // 2. Act
        var spot = await service.GetSpotByIdAsync(spotId);

        // 3. Assert
        Assert.NotNull(spot);
        Assert.Equal("Gokova Bay", spot.Name);
    }

    [Fact]
    public async Task GetSpotByIdAsync_ShouldReturnNull_WhenDoesNotExist()
    {
        // 1. Arrange
        using var db = TestDbContextFactory.Create();
        var service = new SpotService(db);

        // 2. Act
        var spot = await service.GetSpotByIdAsync(Guid.NewGuid());

        // 3. Assert
        Assert.Null(spot);
    }

    [Fact]
    public async Task CreateSpotAsync_ShouldPersistSpot()
    {
        // 1. Arrange
        using var db = TestDbContextFactory.Create();
        var service = new SpotService(db);
        var userId = Guid.NewGuid();

        var dto = new CreateSpotDto
        {
            Name = "Rhodes Theologos",
            Country = "Greece",
            WaterType = WaterType.Chop
        };

        // 2. Act
        var created = await service.CreateSpotAsync(dto, userId);

        // 3. Assert
        Assert.NotNull(created);
        Assert.Equal("Rhodes Theologos", created.Name);
        Assert.Equal(userId, created.CreatedById);

        var inDb = await db.Spots.FindAsync(created.Id);
        Assert.NotNull(inDb);
        Assert.Equal("Greece", inDb.Country);
    }

    [Fact]
    public async Task GetSpotSessionsAsync_ShouldReturnNull_WhenSpotDoesNotExist()
    {
        // 1. Arrange
        using var db = TestDbContextFactory.Create();
        var service = new SpotService(db);

        // 2. Act
        var result = await service.GetSpotSessionsAsync(Guid.NewGuid());

        // 3. Assert
        Assert.Null(result);
    }
}
