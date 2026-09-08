using System.ComponentModel.DataAnnotations;
using KiteSocial.API.Models.Entities;

namespace KiteSocial.API.DTOs.Spots;

public class CreateSpotDto
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Region { get; set; }

    [Range(-90.0, 90.0)]
    public double Latitude { get; set; }

    [Range(-180.0, 180.0)]
    public double Longitude { get; set; }

    public WaterType WaterType { get; set; } = WaterType.Flat;

    [Required]
    [MaxLength(100)]
    public string BestWindDirections { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public string? ImageUrl { get; set; }
}

public class SpotDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Region { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public WaterType WaterType { get; set; }
    public string BestWindDirections { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? CreatedById { get; set; }
    public string? CreatedByName { get; set; }
    public int SessionsCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
