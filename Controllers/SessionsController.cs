using System.Security.Claims;
using KiteSocial.API.Data;
using KiteSocial.API.DTOs.Sessions;
using KiteSocial.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KiteSocial.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    private readonly KiteSocialDbContext _context;

    public SessionsController(KiteSocialDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<KiteSessionDto>>> GetSessions(
        [FromQuery] Guid? spotId,
        [FromQuery] Guid? userId)
    {
        var query = _context.KiteSessions
            .Include(s => s.User)
            .Include(s => s.Spot)
            .AsNoTracking()
            .AsQueryable();

        if (spotId.HasValue)
        {
            query = query.Where(s => s.SpotId == spotId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(s => s.UserId == userId.Value);
        }

        var sessions = await query
            .OrderByDescending(s => s.SessionDate)
            .Select(s => MapToSessionDto(s))
            .ToListAsync();

        return Ok(sessions);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<KiteSessionDto>> GetSession(Guid id)
    {
        var session = await _context.KiteSessions
            .Include(s => s.User)
            .Include(s => s.Spot)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session == null)
        {
            return NotFound(new { Message = "Session not found." });
        }

        return Ok(MapToSessionDto(session));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<KiteSessionDto>> CreateSession([FromBody] CreateKiteSessionDto dto)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            return Unauthorized();
        }

        var spot = await _context.Spots.FindAsync(dto.SpotId);
        if (spot == null)
        {
            return BadRequest(new { Message = "Referenced spot does not exist." });
        }

        var session = new KiteSession
        {
            UserId = userId,
            SpotId = dto.SpotId,
            SessionDate = dto.SessionDate,
            DurationMinutes = dto.DurationMinutes,
            WindSpeedKnots = dto.WindSpeedKnots,
            KiteSize = dto.KiteSize,
            BoardType = dto.BoardType,
            MaxJumpHeightMeters = dto.MaxJumpHeightMeters,
            Notes = dto.Notes,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.KiteSessions.Add(session);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);

        return CreatedAtAction(nameof(GetSession), new { id = session.Id }, new KiteSessionDto
        {
            Id = session.Id,
            UserId = session.UserId,
            UserName = user?.UserName ?? string.Empty,
            UserAvatarUrl = user?.AvatarUrl,
            SpotId = session.SpotId,
            SpotName = spot.Name,
            SpotCountry = spot.Country,
            SessionDate = session.SessionDate,
            DurationMinutes = session.DurationMinutes,
            WindSpeedKnots = session.WindSpeedKnots,
            KiteSize = session.KiteSize,
            BoardType = session.BoardType,
            MaxJumpHeightMeters = session.MaxJumpHeightMeters,
            Notes = session.Notes,
            CreatedAtUtc = session.CreatedAtUtc
        });
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<KiteSessionDto>>> GetMySessions()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            return Unauthorized();
        }

        var sessions = await _context.KiteSessions
            .Include(s => s.User)
            .Include(s => s.Spot)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.SessionDate)
            .Select(s => MapToSessionDto(s))
            .ToListAsync();

        return Ok(sessions);
    }

    [Authorize]
    [HttpGet("stats")]
    public async Task<ActionResult<UserSessionStatsDto>> GetMyStats()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            return Unauthorized();
        }

        var userSessions = await _context.KiteSessions
            .Include(s => s.Spot)
            .Where(s => s.UserId == userId)
            .ToListAsync();

        if (userSessions.Count == 0)
        {
            return Ok(new UserSessionStatsDto
            {
                TotalSessions = 0,
                TotalMinutesOnWater = 0,
                MaxJumpHeightRecord = null,
                FavoriteSpotName = null
            });
        }

        var totalMinutes = userSessions.Sum(s => s.DurationMinutes);
        var maxJump = userSessions.Max(s => s.MaxJumpHeightMeters);
        var favoriteSpot = userSessions
            .GroupBy(s => s.Spot.Name)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        return Ok(new UserSessionStatsDto
        {
            TotalSessions = userSessions.Count,
            TotalMinutesOnWater = totalMinutes,
            MaxJumpHeightRecord = maxJump,
            FavoriteSpotName = favoriteSpot
        });
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSession(Guid id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            return Unauthorized();
        }

        var session = await _context.KiteSessions.FindAsync(id);
        if (session == null)
        {
            return NotFound(new { Message = "Session not found." });
        }

        if (session.UserId != userId)
        {
            return Forbid();
        }

        _context.KiteSessions.Remove(session);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static KiteSessionDto MapToSessionDto(KiteSession s)
    {
        return new KiteSessionDto
        {
            Id = s.Id,
            UserId = s.UserId,
            UserName = s.User?.UserName ?? string.Empty,
            UserAvatarUrl = s.User?.AvatarUrl,
            SpotId = s.SpotId,
            SpotName = s.Spot?.Name ?? string.Empty,
            SpotCountry = s.Spot?.Country ?? string.Empty,
            SessionDate = s.SessionDate,
            DurationMinutes = s.DurationMinutes,
            WindSpeedKnots = s.WindSpeedKnots,
            KiteSize = s.KiteSize,
            BoardType = s.BoardType,
            MaxJumpHeightMeters = s.MaxJumpHeightMeters,
            Notes = s.Notes,
            CreatedAtUtc = s.CreatedAtUtc
        };
    }
}
