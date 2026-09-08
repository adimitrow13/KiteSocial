namespace KiteSocial.API.Models.Entities;

public class KiteSession
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public Guid SpotId { get; set; }
    public Spot Spot { get; set; } = null!;

    public DateTime SessionDate { get; set; } = DateTime.UtcNow;
    public int DurationMinutes { get; set; } // Duration in minutes
    public double? WindSpeedKnots { get; set; }
    public double? KiteSize { get; set; } // Size in m2 (e.g. 9.0, 12.0)
    public BoardType BoardType { get; set; } = BoardType.TwinTip;
    public double? MaxJumpHeightMeters { get; set; } // WOO / Surfr jump height
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
