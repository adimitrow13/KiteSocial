using System.Security.Claims;
using KiteSocial.API.Data;
using KiteSocial.API.DTOs.Comments;
using KiteSocial.API.DTOs.Posts;
using KiteSocial.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KiteSocial.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly KiteSocialDbContext _context;

    public PostsController(KiteSocialDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetPosts(
        [FromQuery] Guid? spotId,
        [FromQuery] Guid? userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var currentUserId = GetCurrentUserId();

        var query = _context.Posts
            .Include(p => p.User)
            .Include(p => p.Spot)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .AsNoTracking()
            .AsQueryable();

        if (spotId.HasValue)
        {
            query = query.Where(p => p.SpotId == spotId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(p => p.UserId == userId.Value);
        }

        pageSize = Math.Clamp(pageSize, 1, 50);
        page = Math.Max(1, page);

        var posts = await query
            .OrderByDescending(p => p.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PostDto
            {
                Id = p.Id,
                UserId = p.UserId,
                UserName = p.User.UserName ?? string.Empty,
                UserAvatarUrl = p.User.AvatarUrl,
                Content = p.Content,
                MediaUrl = p.MediaUrl,
                SpotId = p.SpotId,
                SpotName = p.Spot != null ? p.Spot.Name : null,
                KiteSessionId = p.KiteSessionId,
                LikesCount = p.Likes.Count,
                CommentsCount = p.Comments.Count,
                IsLikedByCurrentUser = currentUserId.HasValue && p.Likes.Any(l => l.UserId == currentUserId.Value),
                CreatedAtUtc = p.CreatedAtUtc
            })
            .ToListAsync();

        return Ok(posts);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostDto>> GetPost(Guid id)
    {
        var currentUserId = GetCurrentUserId();

        var post = await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Spot)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return NotFound(new { Message = "Post not found." });
        }

        return Ok(new PostDto
        {
            Id = post.Id,
            UserId = post.UserId,
            UserName = post.User.UserName ?? string.Empty,
            UserAvatarUrl = post.User.AvatarUrl,
            Content = post.Content,
            MediaUrl = post.MediaUrl,
            SpotId = post.SpotId,
            SpotName = post.Spot?.Name,
            KiteSessionId = post.KiteSessionId,
            LikesCount = post.Likes.Count,
            CommentsCount = post.Comments.Count,
            IsLikedByCurrentUser = currentUserId.HasValue && post.Likes.Any(l => l.UserId == currentUserId.Value),
            CreatedAtUtc = post.CreatedAtUtc
        });
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<PostDto>> CreatePost([FromBody] CreatePostDto dto)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        if (dto.SpotId.HasValue && !await _context.Spots.AnyAsync(s => s.Id == dto.SpotId.Value))
        {
            return BadRequest(new { Message = "Referenced spot does not exist." });
        }

        if (dto.KiteSessionId.HasValue && !await _context.KiteSessions.AnyAsync(s => s.Id == dto.KiteSessionId.Value))
        {
            return BadRequest(new { Message = "Referenced session does not exist." });
        }

        var post = new Post
        {
            UserId = userId.Value,
            Content = dto.Content,
            MediaUrl = dto.MediaUrl,
            SpotId = dto.SpotId,
            KiteSessionId = dto.KiteSessionId,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId.Value);
        string? spotName = null;
        if (dto.SpotId.HasValue)
        {
            var spot = await _context.Spots.FindAsync(dto.SpotId.Value);
            spotName = spot?.Name;
        }

        return CreatedAtAction(nameof(GetPost), new { id = post.Id }, new PostDto
        {
            Id = post.Id,
            UserId = post.UserId,
            UserName = user?.UserName ?? string.Empty,
            UserAvatarUrl = user?.AvatarUrl,
            Content = post.Content,
            MediaUrl = post.MediaUrl,
            SpotId = post.SpotId,
            SpotName = spotName,
            KiteSessionId = post.KiteSessionId,
            LikesCount = 0,
            CommentsCount = 0,
            IsLikedByCurrentUser = false,
            CreatedAtUtc = post.CreatedAtUtc
        });
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var post = await _context.Posts.FindAsync(id);
        if (post == null)
        {
            return NotFound(new { Message = "Post not found." });
        }

        if (post.UserId != userId.Value)
        {
            return Forbid();
        }

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [Authorize]
    [HttpPost("{id:guid}/like")]
    public async Task<ActionResult<LikeResponseDto>> ToggleLike(Guid id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var postExists = await _context.Posts.AnyAsync(p => p.Id == id);
        if (!postExists)
        {
            return NotFound(new { Message = "Post not found." });
        }

        var existingLike = await _context.Likes
            .FirstOrDefaultAsync(l => l.PostId == id && l.UserId == userId.Value);

        bool isLiked;
        if (existingLike != null)
        {
            _context.Likes.Remove(existingLike);
            isLiked = false;
        }
        else
        {
            _context.Likes.Add(new Like
            {
                PostId = id,
                UserId = userId.Value,
                CreatedAtUtc = DateTime.UtcNow
            });
            isLiked = true;
        }

        await _context.SaveChangesAsync();

        var likesCount = await _context.Likes.CountAsync(l => l.PostId == id);

        return Ok(new LikeResponseDto
        {
            PostId = id,
            LikesCount = likesCount,
            IsLiked = isLiked
        });
    }

    [HttpGet("{id:guid}/comments")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetComments(Guid id)
    {
        var postExists = await _context.Posts.AnyAsync(p => p.Id == id);
        if (!postExists)
        {
            return NotFound(new { Message = "Post not found." });
        }

        var comments = await _context.Comments
            .Include(c => c.User)
            .Where(c => c.PostId == id)
            .OrderBy(c => c.CreatedAtUtc)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                PostId = c.PostId,
                UserId = c.UserId,
                UserName = c.User.UserName ?? string.Empty,
                UserAvatarUrl = c.User.AvatarUrl,
                Content = c.Content,
                CreatedAtUtc = c.CreatedAtUtc
            })
            .ToListAsync();

        return Ok(comments);
    }

    [Authorize]
    [HttpPost("{id:guid}/comments")]
    public async Task<ActionResult<CommentDto>> AddComment(Guid id, [FromBody] CreateCommentDto dto)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var postExists = await _context.Posts.AnyAsync(p => p.Id == id);
        if (!postExists)
        {
            return NotFound(new { Message = "Post not found." });
        }

        var comment = new Comment
        {
            PostId = id,
            UserId = userId.Value,
            Content = dto.Content,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId.Value);

        return Ok(new CommentDto
        {
            Id = comment.Id,
            PostId = comment.PostId,
            UserId = comment.UserId,
            UserName = user?.UserName ?? string.Empty,
            UserAvatarUrl = user?.AvatarUrl,
            Content = comment.Content,
            CreatedAtUtc = comment.CreatedAtUtc
        });
    }

    private Guid? GetCurrentUserId()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdStr, out var id) ? id : null;
    }
}
