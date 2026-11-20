using System.Security.Claims;
using KiteSocial.API.DTOs.Sessions;
using KiteSocial.API.DTOs.Spots;
using KiteSocial.API.Models.Entities;
using KiteSocial.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KiteSocial.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpotsController : ControllerBase
{
    private readonly ISpotService _spotService;

    public SpotsController(ISpotService spotService)
    {
        _spotService = spotService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SpotDto>>> GetSpots(
        [FromQuery] string? search,
        [FromQuery] string? country,
        [FromQuery] WaterType? waterType)
    {
        var spots = await _spotService.GetAllSpotsAsync(search, country, waterType);
        return Ok(spots);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SpotDto>> GetSpot(Guid id)
    {
        var spot = await _spotService.GetSpotByIdAsync(id);
        if (spot == null)
        {
            return NotFound(new { Message = "Spot not found." });
        }

        return Ok(spot);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<SpotDto>> CreateSpot([FromBody] CreateSpotDto dto)
    {
        var userIdStr = User?.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid? userId = Guid.TryParse(userIdStr, out var parsedGuid) ? parsedGuid : null;

        var createdSpot = await _spotService.CreateSpotAsync(dto, userId);
        return CreatedAtAction(nameof(GetSpot), new { id = createdSpot.Id }, createdSpot);
    }

    [HttpGet("{id:guid}/sessions")]
    public async Task<ActionResult<IEnumerable<KiteSessionDto>>> GetSpotSessions(Guid id)
    {
        var sessions = await _spotService.GetSpotSessionsAsync(id);
        if (sessions == null)
        {
            return NotFound(new { Message = "Spot not found." });
        }

        return Ok(sessions);
    }
}
