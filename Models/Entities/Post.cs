namespace KiteSocial.API.Models.Entities;

public class Post
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string Content { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }

    public Guid? SpotId { get; set; }
    public Spot? Spot { get; set; }

    public Guid? KiteSessionId { get; set; }
    public KiteSession? KiteSession { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Like> Likes { get; set; } = new List<Like>();
}
