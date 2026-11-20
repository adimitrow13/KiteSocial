using KiteSocial.API.Controllers;
using KiteSocial.API.DTOs.Sessions;
using KiteSocial.API.Services.Interfaces;
using KiteSocial.Tests.Common;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace KiteSocial.Tests.Controllers;

public class SessionsControllerTests
{
    [Fact]
    public async Task DeleteSession_ShouldReturnForbid_WhenServiceReturnsForbidden()
    {
        // 1. Arrange
        var sessionId = Guid.NewGuid();
        var loggedUserId = Guid.NewGuid();

        var mockService = new Mock<ISessionService>();
        mockService
            .Setup(s => s.DeleteSessionAsync(sessionId, loggedUserId))
            .ReturnsAsync(SessionDeleteResult.Forbidden);

        var controller = new SessionsController(mockService.Object);
        ControllerTestHelper.SetLoggedUser(controller, loggedUserId);

        // 2. Act
        var result = await controller.DeleteSession(sessionId);

        // 3. Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task DeleteSession_ShouldReturnNotFound_WhenServiceReturnsNotFound()
    {
        // 1. Arrange
        var sessionId = Guid.NewGuid();
        var loggedUserId = Guid.NewGuid();

        var mockService = new Mock<ISessionService>();
        mockService
            .Setup(s => s.DeleteSessionAsync(sessionId, loggedUserId))
            .ReturnsAsync(SessionDeleteResult.NotFound);

        var controller = new SessionsController(mockService.Object);
        ControllerTestHelper.SetLoggedUser(controller, loggedUserId);

        // 2. Act
        var result = await controller.DeleteSession(sessionId);

        // 3. Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteSession_ShouldReturnNoContent_WhenServiceReturnsSuccess()
    {
        // 1. Arrange
        var sessionId = Guid.NewGuid();
        var loggedUserId = Guid.NewGuid();

        var mockService = new Mock<ISessionService>();
        mockService
            .Setup(s => s.DeleteSessionAsync(sessionId, loggedUserId))
            .ReturnsAsync(SessionDeleteResult.Success);

        var controller = new SessionsController(mockService.Object);
        ControllerTestHelper.SetLoggedUser(controller, loggedUserId);

        // 2. Act
        var result = await controller.DeleteSession(sessionId);

        // 3. Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task GetMyStats_ShouldReturnOkWithStats()
    {
        // 1. Arrange
        var loggedUserId = Guid.NewGuid();
        var expectedStats = new UserSessionStatsDto
        {
            TotalSessions = 5,
            TotalMinutesOnWater = 300,
            MaxJumpHeightRecord = 11.2,
            FavoriteSpotName = "Gradina"
        };

        var mockService = new Mock<ISessionService>();
        mockService
            .Setup(s => s.GetUserStatsAsync(loggedUserId))
            .ReturnsAsync(expectedStats);

        var controller = new SessionsController(mockService.Object);
        ControllerTestHelper.SetLoggedUser(controller, loggedUserId);

        // 2. Act
        var actionResult = await controller.GetMyStats();

        // 3. Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var stats = Assert.IsType<UserSessionStatsDto>(okResult.Value);
        Assert.Equal(5, stats.TotalSessions);
        Assert.Equal("Gradina", stats.FavoriteSpotName);
    }
}
