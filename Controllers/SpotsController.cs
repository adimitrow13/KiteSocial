using System.Security.Claims;
using KiteSocial.API.Data;
using KiteSocial.API.DTOs.Sessions;
using KiteSocial.API.DTOs.Spots;
using KiteSocial.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KiteSocial.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpotsController : ControllerBase
{
    private readonly KiteSocialDbContext _context;

    public SpotsController(KiteSocialDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SpotDto>>> GetSpots(
        [FromQuery] string? search,
        [FromQuery] string? country,
        [FromQuery] WaterType? waterType)
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

        var spots = await query
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

        return Ok(spots);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SpotDto>> GetSpot(Guid id)
    {
        var spot = await _context.Spots
            .Include(s => s.CreatedBy)
            .Include(s => s.Sessions)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (spot == null)
        {
            return NotFound(new { Message = "Spot not found." });
        }

        return Ok(new SpotDto
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
        });
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<SpotDto>> CreateSpot([FromBody] CreateSpotDto dto)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid.TryParse(userIdStr, out var userId);

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
            CreatedById = userId != Guid.Empty ? userId : null,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Spots.Add(spot);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSpot), new { id = spot.Id }, new SpotDto
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
        });
    }

    [HttpGet("{id:guid}/sessions")]
    public async Task<ActionResult<IEnumerable<KiteSessionDto>>> GetSpotSessions(Guid id)
    {
        var spotExists = await _context.Spots.AnyAsync(s => s.Id == id);
        if (!spotExists)
        {
            return NotFound(new { Message = "Spot not found." });
        }

        var sessions = await _context.KiteSessions
            .Include(s => s.User)
            .Include(s => s.Spot)
            .Where(s => s.SpotId == id)
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

        return Ok(sessions);
    }
}
