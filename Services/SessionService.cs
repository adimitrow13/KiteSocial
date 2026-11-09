using KiteSocial.API.Data;
using KiteSocial.API.DTOs.Sessions;
using KiteSocial.API.Models.Entities;
using KiteSocial.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KiteSocial.API.Services;

public class SessionService : ISessionService
{
    private readonly KiteSocialDbContext _context;

    public SessionService(KiteSocialDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<KiteSessionDto>> GetSessionsAsync(Guid? spotId, Guid? userId)
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

        return await query
            .OrderByDescending(s => s.SessionDate)
            .Select(s => MapToSessionDto(s))
            .ToListAsync();
    }

    public async Task<KiteSessionDto?> GetSessionByIdAsync(Guid id)
    {
        var session = await _context.KiteSessions
            .Include(s => s.User)
            .Include(s => s.Spot)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        return session == null ? null : MapToSessionDto(session);
    }

    public async Task<KiteSessionDto?> CreateSessionAsync(CreateKiteSessionDto dto, Guid userId)
    {
        var spot = await _context.Spots.FindAsync(dto.SpotId);
        if (spot == null) return null;

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

        return new KiteSessionDto
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
        };
    }

    public async Task<IEnumerable<KiteSessionDto>> GetUserSessionsAsync(Guid userId)
    {
        return await _context.KiteSessions
            .Include(s => s.User)
            .Include(s => s.Spot)
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.SessionDate)
            .Select(s => MapToSessionDto(s))
            .ToListAsync();
    }

    public async Task<UserSessionStatsDto> GetUserStatsAsync(Guid userId)
    {
        var userSessions = await _context.KiteSessions
            .Include(s => s.Spot)
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .ToListAsync();

        if (userSessions.Count == 0)
        {
            return new UserSessionStatsDto
            {
                TotalSessions = 0,
                TotalMinutesOnWater = 0,
                MaxJumpHeightRecord = null,
                FavoriteSpotName = null
            };
        }

        var totalMinutes = userSessions.Sum(s => s.DurationMinutes);
        var maxJump = userSessions.Max(s => s.MaxJumpHeightMeters);
        var favoriteSpot = userSessions
            .GroupBy(s => s.Spot.Name)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        return new UserSessionStatsDto
        {
            TotalSessions = userSessions.Count,
            TotalMinutesOnWater = totalMinutes,
            MaxJumpHeightRecord = maxJump,
            FavoriteSpotName = favoriteSpot
        };
    }

    public async Task<SessionDeleteResult> DeleteSessionAsync(Guid id, Guid userId)
    {
        var session = await _context.KiteSessions.FindAsync(id);
        if (session == null)
        {
            return SessionDeleteResult.NotFound;
        }

        if (session.UserId != userId)
        {
            return SessionDeleteResult.Forbidden;
        }

        _context.KiteSessions.Remove(session);
        await _context.SaveChangesAsync();
        return SessionDeleteResult.Success;
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
