using KiteSocial.API.DTOs.Comments;
using KiteSocial.API.DTOs.Posts;

namespace KiteSocial.API.Services.Interfaces;

public enum PostDeleteResult
{
    Success,
    NotFound,
    Forbidden
}

public interface IPostService
{
    Task<IEnumerable<PostDto>> GetPostsAsync(Guid? spotId, Guid? userId, int page, int pageSize, Guid? currentUserId);
    Task<PostDto?> GetPostByIdAsync(Guid id, Guid? currentUserId);
    Task<(PostDto? Post, string? ErrorMessage)> CreatePostAsync(CreatePostDto dto, Guid userId);
    Task<PostDeleteResult> DeletePostAsync(Guid id, Guid userId);
    Task<LikeResponseDto?> ToggleLikeAsync(Guid postId, Guid userId);
    Task<IEnumerable<CommentDto>?> GetCommentsAsync(Guid postId);
    Task<CommentDto?> AddCommentAsync(Guid postId, CreateCommentDto dto, Guid userId);
    Task<PostDeleteResult> DeleteCommentAsync(Guid postId, Guid commentId, Guid userId);
}
