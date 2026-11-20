using KiteSocial.API.Controllers;
using KiteSocial.API.DTOs.Sessions;
using KiteSocial.API.DTOs.Spots;
using KiteSocial.API.Models.Entities;
using KiteSocial.API.Services.Interfaces;
using KiteSocial.Tests.Common;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace KiteSocial.Tests.Controllers;

public class SpotsControllerTests
{
    [Fact]
    public async Task GetSpots_ShouldReturnOk_WithSpotsList()
    {
        // 1. Arrange
        var mockService = new Mock<ISpotService>();
        var fakeSpots = new List<SpotDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Gradina", Country = "Bulgaria", WaterType = WaterType.Flat },
            new() { Id = Guid.NewGuid(), Name = "Gokova", Country = "Turkey", WaterType = WaterType.Flat }
        };

        mockService
            .Setup(s => s.GetAllSpotsAsync(null, null, null))
            .ReturnsAsync(fakeSpots);

        var controller = new SpotsController(mockService.Object);

        // 2. Act
        var result = await controller.GetSpots(null, null, null);

        // 3. Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedSpots = Assert.IsAssignableFrom<IEnumerable<SpotDto>>(okResult.Value);
        Assert.Equal(2, returnedSpots.Count());
    }

    [Fact]
    public async Task GetSpot_ShouldReturnNotFound_WhenSpotDoesNotExist()
    {
        // 1. Arrange
        var spotId = Guid.NewGuid();
        var mockService = new Mock<ISpotService>();
        mockService
            .Setup(s => s.GetSpotByIdAsync(spotId))
            .ReturnsAsync((SpotDto?)null);

        var controller = new SpotsController(mockService.Object);

        // 2. Act
        var result = await controller.GetSpot(spotId);

        // 3. Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetSpot_ShouldReturnOk_WhenSpotExists()
    {
        // 1. Arrange
        var spotId = Guid.NewGuid();
        var spotDto = new SpotDto
        {
            Id = spotId,
            Name = "Gradina",
            Country = "Bulgaria"
        };

        var mockService = new Mock<ISpotService>();
        mockService
            .Setup(s => s.GetSpotByIdAsync(spotId))
            .ReturnsAsync(spotDto);

        var controller = new SpotsController(mockService.Object);

        // 2. Act
        var result = await controller.GetSpot(spotId);

        // 3. Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedSpot = Assert.IsType<SpotDto>(okResult.Value);
        Assert.Equal(spotId, returnedSpot.Id);
        Assert.Equal("Gradina", returnedSpot.Name);
    }

    [Fact]
    public async Task CreateSpot_ShouldReturnCreatedAtAction_WithCreatedSpot()
    {
        // 1. Arrange
        var userId = Guid.NewGuid();
        var createDto = new CreateSpotDto
        {
            Name = "Kavatsi",
            Country = "Bulgaria",
            Latitude = 42.4,
            Longitude = 27.7,
            BestWindDirections = "NE, E"
        };

        var createdSpotDto = new SpotDto
        {
            Id = Guid.NewGuid(),
            Name = createDto.Name,
            Country = createDto.Country,
            CreatedById = userId
        };

        var mockService = new Mock<ISpotService>();
        mockService
            .Setup(s => s.CreateSpotAsync(createDto, userId))
            .ReturnsAsync(createdSpotDto);

        var controller = new SpotsController(mockService.Object);
        ControllerTestHelper.SetLoggedUser(controller, userId);

        // 2. Act
        var result = await controller.CreateSpot(createDto);

        // 3. Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(SpotsController.GetSpot), createdResult.ActionName);
        Assert.Equal(createdSpotDto.Id, createdResult.RouteValues?["id"]);
        var returnedSpot = Assert.IsType<SpotDto>(createdResult.Value);
        Assert.Equal("Kavatsi", returnedSpot.Name);
    }

    [Fact]
    public async Task GetSpotSessions_ShouldReturnNotFound_WhenSpotDoesNotExist()
    {
        // 1. Arrange
        var spotId = Guid.NewGuid();
        var mockService = new Mock<ISpotService>();
        mockService
            .Setup(s => s.GetSpotSessionsAsync(spotId))
            .ReturnsAsync((IEnumerable<KiteSessionDto>?)null);

        var controller = new SpotsController(mockService.Object);

        // 2. Act
        var result = await controller.GetSpotSessions(spotId);

        // 3. Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetSpotSessions_ShouldReturnOk_WhenSpotExists()
    {
        // 1. Arrange
        var spotId = Guid.NewGuid();
        var fakeSessions = new List<KiteSessionDto>
        {
            new() { Id = Guid.NewGuid(), SpotId = spotId, SpotName = "Gradina" }
        };

        var mockService = new Mock<ISpotService>();
        mockService
            .Setup(s => s.GetSpotSessionsAsync(spotId))
            .ReturnsAsync(fakeSessions);

        var controller = new SpotsController(mockService.Object);

        // 2. Act
        var result = await controller.GetSpotSessions(spotId);

        // 3. Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedSessions = Assert.IsAssignableFrom<IEnumerable<KiteSessionDto>>(okResult.Value);
        Assert.Single(returnedSessions);
    }
}
