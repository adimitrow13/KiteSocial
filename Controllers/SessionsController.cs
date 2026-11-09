using System.Security.Claims;
using KiteSocial.API.DTOs.Sessions;
using KiteSocial.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KiteSocial.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;

    public SessionsController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<KiteSessionDto>>> GetSessions(
        [FromQuery] Guid? spotId,
        [FromQuery] Guid? userId)
    {
        var sessions = await _sessionService.GetSessionsAsync(spotId, userId);
        return Ok(sessions);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<KiteSessionDto>> GetSession(Guid id)
    {
        var session = await _sessionService.GetSessionByIdAsync(id);
        if (session == null)
        {
            return NotFound(new { Message = "Session not found." });
        }

        return Ok(session);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<KiteSessionDto>> CreateSession([FromBody] CreateKiteSessionDto dto)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized();

        var createdSession = await _sessionService.CreateSessionAsync(dto, userId.Value);
        if (createdSession == null)
        {
            return BadRequest(new { Message = "Referenced spot does not exist." });
        }

        return CreatedAtAction(nameof(GetSession), new { id = createdSession.Id }, createdSession);
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<KiteSessionDto>>> GetMySessions()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized();

        var sessions = await _sessionService.GetUserSessionsAsync(userId.Value);
        return Ok(sessions);
    }

    [Authorize]
    [HttpGet("stats")]
    public async Task<ActionResult<UserSessionStatsDto>> GetMyStats()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized();

        var stats = await _sessionService.GetUserStatsAsync(userId.Value);
        return Ok(stats);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSession(Guid id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized();

        var result = await _sessionService.DeleteSessionAsync(id, userId.Value);
        return result switch
        {
            SessionDeleteResult.NotFound => NotFound(new { Message = "Session not found." }),
            SessionDeleteResult.Forbidden => Forbid(),
            SessionDeleteResult.Success => NoContent(),
            _ => BadRequest()
        };
    }

    private Guid? GetCurrentUserId()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdStr, out var userId) ? userId : null;
    }
}
