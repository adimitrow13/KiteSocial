using KiteSocial.API.Data;
using KiteSocial.API.DTOs.Comments;
using KiteSocial.API.DTOs.Posts;
using KiteSocial.API.Models.Entities;
using KiteSocial.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KiteSocial.API.Services;

public class PostService : IPostService
{
    private readonly KiteSocialDbContext _context;

    public PostService(KiteSocialDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PostDto>> GetPostsAsync(Guid? spotId, Guid? userId, int page, int pageSize, Guid? currentUserId)
    {
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

        return await query
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
    }

    public async Task<PostDto?> GetPostByIdAsync(Guid id, Guid? currentUserId)
    {
        var post = await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Spot)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null) return null;

        return new PostDto
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
        };
    }

    public async Task<(PostDto? Post, string? ErrorMessage)> CreatePostAsync(CreatePostDto dto, Guid userId)
    {
        if (dto.SpotId.HasValue && !await _context.Spots.AnyAsync(s => s.Id == dto.SpotId.Value))
        {
            return (null, "Referenced spot does not exist.");
        }

        if (dto.KiteSessionId.HasValue && !await _context.KiteSessions.AnyAsync(s => s.Id == dto.KiteSessionId.Value))
        {
            return (null, "Referenced session does not exist.");
        }

        var post = new Post
        {
            UserId = userId,
            Content = dto.Content,
            MediaUrl = dto.MediaUrl,
            SpotId = dto.SpotId,
            KiteSessionId = dto.KiteSessionId,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);
        string? spotName = null;
        if (post.SpotId.HasValue)
        {
            var spot = await _context.Spots.FindAsync(post.SpotId.Value);
            spotName = spot?.Name;
        }

        return (new PostDto
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
        }, null);
    }

    public async Task<PostDeleteResult> DeletePostAsync(Guid id, Guid userId)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post == null) return PostDeleteResult.NotFound;
        if (post.UserId != userId) return PostDeleteResult.Forbidden;

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();
        return PostDeleteResult.Success;
    }

    public async Task<LikeResponseDto?> ToggleLikeAsync(Guid postId, Guid userId)
    {
        var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
        if (!postExists) return null;

        var existingLike = await _context.Likes
            .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

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
                PostId = postId,
                UserId = userId,
                CreatedAtUtc = DateTime.UtcNow
            });
            isLiked = true;
        }

        await _context.SaveChangesAsync();

        var likesCount = await _context.Likes.CountAsync(l => l.PostId == postId);
        return new LikeResponseDto
        {
            PostId = postId,
            LikesCount = likesCount,
            IsLiked = isLiked
        };
    }

    public async Task<IEnumerable<CommentDto>?> GetCommentsAsync(Guid postId)
    {
        var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
        if (!postExists) return null;

        return await _context.Comments
            .Include(c => c.User)
            .AsNoTracking()
            .Where(c => c.PostId == postId)
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
    }

    public async Task<CommentDto?> AddCommentAsync(Guid postId, CreateCommentDto dto, Guid userId)
    {
        var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
        if (!postExists) return null;

        var comment = new Comment
        {
            PostId = postId,
            UserId = userId,
            Content = dto.Content,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);

        return new CommentDto
        {
            Id = comment.Id,
            PostId = comment.PostId,
            UserId = comment.UserId,
            UserName = user?.UserName ?? string.Empty,
            UserAvatarUrl = user?.AvatarUrl,
            Content = comment.Content,
            CreatedAtUtc = comment.CreatedAtUtc
        };
    }

    public async Task<PostDeleteResult> DeleteCommentAsync(Guid postId, Guid commentId, Guid userId)
    {
        var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId && c.PostId == postId);
        if (comment == null) return PostDeleteResult.NotFound;
        if (comment.UserId != userId) return PostDeleteResult.Forbidden;

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
        return PostDeleteResult.Success;
    }
}
