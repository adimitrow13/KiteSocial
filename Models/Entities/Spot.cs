namespace KiteSocial.API.Models.Entities;

public class Spot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Region { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public WaterType WaterType { get; set; } = WaterType.Flat;
    public string BestWindDirections { get; set; } = string.Empty; // e.g., "N, NE, E"
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Guid? CreatedById { get; set; }
    public ApplicationUser? CreatedBy { get; set; }

    public ICollection<KiteSession> Sessions { get; set; } = new List<KiteSession>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
