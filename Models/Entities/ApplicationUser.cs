using Microsoft.AspNetCore.Identity;

namespace KiteSocial.API.Models.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public SkillLevel SkillLevel { get; set; } = SkillLevel.Intermediate;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Guid? HomeSpotId { get; set; }
    public Spot? HomeSpot { get; set; }

    public ICollection<KiteSession> Sessions { get; set; } = new List<KiteSession>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Like> Likes { get; set; } = new List<Like>();
}
