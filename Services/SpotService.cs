using KiteSocial.API.Data;
using KiteSocial.API.DTOs.Sessions;
using KiteSocial.API.DTOs.Spots;
using KiteSocial.API.Models.Entities;
using KiteSocial.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KiteSocial.API.Services;

public class SpotService : ISpotService
{
    private readonly KiteSocialDbContext _context;

    public SpotService(KiteSocialDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SpotDto>> GetAllSpotsAsync(string? search, string? country, WaterType? waterType)
    {
        var query = _context.Spots
            .Include(s => s.CreatedBy)
            .Include(s => s.Sessions)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(searchLower)
                                  || s.Country.ToLower().Contains(searchLower)
                                  || (s.Region != null && s.Region.ToLower().Contains(searchLower)));
        }

        if (!string.IsNullOrWhiteSpace(country))
        {
            query = query.Where(s => s.Country.ToLower() == country.Trim().ToLower());
        }

        if (waterType.HasValue)
        {
            query = query.Where(s => s.WaterType == waterType.Value);
        }

        return await query
            .OrderBy(s => s.Name)
            .Select(s => new SpotDto
            {
                Id = s.Id,
                Name = s.Name,
                Country = s.Country,
                Region = s.Region,
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                WaterType = s.WaterType,
                BestWindDirections = s.BestWindDirections,
                Description = s.Description,
                ImageUrl = s.ImageUrl,
                CreatedById = s.CreatedById,
                CreatedByName = s.CreatedBy != null ? s.CreatedBy.FullName : null,
                SessionsCount = s.Sessions.Count,
                CreatedAtUtc = s.CreatedAtUtc
            })
            .ToListAsync();
    }

    public async Task<SpotDto?> GetSpotByIdAsync(Guid id)
    {
        var spot = await _context.Spots
            .Include(s => s.CreatedBy)
            .Include(s => s.Sessions)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (spot == null) return null;

        return new SpotDto
        {
            Id = spot.Id,
            Name = spot.Name,
            Country = spot.Country,
            Region = spot.Region,
            Latitude = spot.Latitude,
            Longitude = spot.Longitude,
            WaterType = spot.WaterType,
            BestWindDirections = spot.BestWindDirections,
            Description = spot.Description,
            ImageUrl = spot.ImageUrl,
            CreatedById = spot.CreatedById,
            CreatedByName = spot.CreatedBy?.FullName,
            SessionsCount = spot.Sessions.Count,
            CreatedAtUtc = spot.CreatedAtUtc
        };
    }

    public async Task<SpotDto> CreateSpotAsync(CreateSpotDto dto, Guid? userId)
    {
        var spot = new Spot
        {
            Name = dto.Name,
            Country = dto.Country,
            Region = dto.Region,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            WaterType = dto.WaterType,
            BestWindDirections = dto.BestWindDirections,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            CreatedById = userId,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Spots.Add(spot);
        await _context.SaveChangesAsync();

        return new SpotDto
        {
            Id = spot.Id,
            Name = spot.Name,
            Country = spot.Country,
            Region = spot.Region,
            Latitude = spot.Latitude,
            Longitude = spot.Longitude,
            WaterType = spot.WaterType,
            BestWindDirections = spot.BestWindDirections,
            Description = spot.Description,
            ImageUrl = spot.ImageUrl,
            CreatedById = spot.CreatedById,
            SessionsCount = 0,
            CreatedAtUtc = spot.CreatedAtUtc
        };
    }

    public async Task<IEnumerable<KiteSessionDto>?> GetSpotSessionsAsync(Guid spotId)
    {
        var spotExists = await _context.Spots.AnyAsync(s => s.Id == spotId);
        if (!spotExists) return null;

        return await _context.KiteSessions
            .Include(s => s.User)
            .Include(s => s.Spot)
            .Where(s => s.SpotId == spotId)
            .OrderByDescending(s => s.SessionDate)
            .Select(s => new KiteSessionDto
            {
                Id = s.Id,
                UserId = s.UserId,
                UserName = s.User.UserName ?? string.Empty,
                UserAvatarUrl = s.User.AvatarUrl,
                SpotId = s.SpotId,
                SpotName = s.Spot.Name,
                SpotCountry = s.Spot.Country,
                SessionDate = s.SessionDate,
                DurationMinutes = s.DurationMinutes,
                WindSpeedKnots = s.WindSpeedKnots,
                KiteSize = s.KiteSize,
                BoardType = s.BoardType,
                MaxJumpHeightMeters = s.MaxJumpHeightMeters,
                Notes = s.Notes,
                CreatedAtUtc = s.CreatedAtUtc
            })
            .ToListAsync();
    }
}
