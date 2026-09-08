using System.ComponentModel.DataAnnotations;

namespace KiteSocial.API.DTOs.Posts;

public class CreatePostDto
{
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public string? MediaUrl { get; set; }

    public Guid? SpotId { get; set; }

    public Guid? KiteSessionId { get; set; }
}

public class PostDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatarUrl { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public Guid? SpotId { get; set; }
    public string? SpotName { get; set; }
    public Guid? KiteSessionId { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class LikeResponseDto
{
    public Guid PostId { get; set; }
    public int LikesCount { get; set; }
    public bool IsLiked { get; set; }
}
