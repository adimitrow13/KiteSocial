using System.ComponentModel.DataAnnotations;
using KiteSocial.API.Models.Entities;

namespace KiteSocial.API.DTOs.Sessions;

public class CreateKiteSessionDto
{
    [Required]
    public Guid SpotId { get; set; }

    public DateTime SessionDate { get; set; } = DateTime.UtcNow;

    [Range(1, 1440, ErrorMessage = "Duration must be between 1 minute and 24 hours.")]
    public int DurationMinutes { get; set; }

    [Range(0, 100)]
    public double? WindSpeedKnots { get; set; }

    [Range(1.0, 30.0)]
    public double? KiteSize { get; set; }

    public BoardType BoardType { get; set; } = BoardType.TwinTip;

    [Range(0, 50.0)]
    public double? MaxJumpHeightMeters { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}

public class KiteSessionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatarUrl { get; set; }
    public Guid SpotId { get; set; }
    public string SpotName { get; set; } = string.Empty;
    public string SpotCountry { get; set; } = string.Empty;
    public DateTime SessionDate { get; set; }
    public int DurationMinutes { get; set; }
    public double? WindSpeedKnots { get; set; }
    public double? KiteSize { get; set; }
    public BoardType BoardType { get; set; }
    public double? MaxJumpHeightMeters { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class UserSessionStatsDto
{
    public int TotalSessions { get; set; }
    public int TotalMinutesOnWater { get; set; }
    public double TotalHoursOnWater => Math.Round(TotalMinutesOnWater / 60.0, 1);
    public double? MaxJumpHeightRecord { get; set; }
    public string? FavoriteSpotName { get; set; }
}
