using System.Security.Claims;
using KiteSocial.API.DTOs.Comments;
using KiteSocial.API.DTOs.Posts;
using KiteSocial.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KiteSocial.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetPosts(
        [FromQuery] Guid? spotId,
        [FromQuery] Guid? userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var currentUserId = GetCurrentUserId();
        var posts = await _postService.GetPostsAsync(spotId, userId, page, pageSize, currentUserId);
        return Ok(posts);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostDto>> GetPost(Guid id)
    {
        var currentUserId = GetCurrentUserId();
        var post = await _postService.GetPostByIdAsync(id, currentUserId);

        if (post == null)
        {
            return NotFound(new { Message = "Post not found." });
        }

        return Ok(post);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<PostDto>> CreatePost([FromBody] CreatePostDto dto)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized();

        var (post, errorMessage) = await _postService.CreatePostAsync(dto, userId.Value);
        if (errorMessage != null)
        {
            return BadRequest(new { Message = errorMessage });
        }

        return CreatedAtAction(nameof(GetPost), new { id = post!.Id }, post);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized();

        var result = await _postService.DeletePostAsync(id, userId.Value);
        return result switch
        {
            PostDeleteResult.NotFound => NotFound(new { Message = "Post not found." }),
            PostDeleteResult.Forbidden => Forbid(),
            PostDeleteResult.Success => NoContent(),
            _ => BadRequest()
        };
    }

    [Authorize]
    [HttpPost("{id:guid}/like")]
    public async Task<ActionResult<LikeResponseDto>> ToggleLike(Guid id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized();

        var likeResponse = await _postService.ToggleLikeAsync(id, userId.Value);
        if (likeResponse == null)
        {
            return NotFound(new { Message = "Post not found." });
        }

        return Ok(likeResponse);
    }

    [HttpGet("{id:guid}/comments")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetComments(Guid id)
    {
        var comments = await _postService.GetCommentsAsync(id);
        if (comments == null)
        {
            return NotFound(new { Message = "Post not found." });
        }

        return Ok(comments);
    }

    [Authorize]
    [HttpPost("{id:guid}/comments")]
    public async Task<ActionResult<CommentDto>> AddComment(Guid id, [FromBody] CreateCommentDto dto)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized();

        var comment = await _postService.AddCommentAsync(id, dto, userId.Value);
        if (comment == null)
        {
            return NotFound(new { Message = "Post not found." });
        }

        return CreatedAtAction(nameof(GetPost), new { id }, comment);
    }

    [Authorize]
    [HttpDelete("{id:guid}/comments/{commentId:guid}")]
    public async Task<IActionResult> DeleteComment(Guid id, Guid commentId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue) return Unauthorized();

        var result = await _postService.DeleteCommentAsync(id, commentId, userId.Value);
        return result switch
        {
            PostDeleteResult.NotFound => NotFound(new { Message = "Comment not found." }),
            PostDeleteResult.Forbidden => Forbid(),
            PostDeleteResult.Success => NoContent(),
            _ => BadRequest()
        };
    }

    private Guid? GetCurrentUserId()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdStr, out var userId) ? userId : null;
    }
}
