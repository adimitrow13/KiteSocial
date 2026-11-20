using KiteSocial.API.Controllers;
using KiteSocial.API.DTOs.Comments;
using KiteSocial.API.DTOs.Posts;
using KiteSocial.API.Services.Interfaces;
using KiteSocial.Tests.Common;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace KiteSocial.Tests.Controllers;

public class PostsControllerTests
{
    [Fact]
    public async Task GetPosts_ShouldReturnOk_WithPostsList()
    {
        // 1. Arrange
        var mockService = new Mock<IPostService>();
        var fakePosts = new List<PostDto>
        {
            new() { Id = Guid.NewGuid(), Content = "Amazing kite session!" },
            new() { Id = Guid.NewGuid(), Content = "Wind was 25 knots today." }
        };

        mockService
            .Setup(s => s.GetPostsAsync(null, null, 1, 20, null))
            .ReturnsAsync(fakePosts);

        var controller = new PostsController(mockService.Object);

        // 2. Act
        var result = await controller.GetPosts(null, null, 1, 20);

        // 3. Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedPosts = Assert.IsAssignableFrom<IEnumerable<PostDto>>(okResult.Value);
        Assert.Equal(2, returnedPosts.Count());
    }

    [Fact]
    public async Task GetPost_ShouldReturnNotFound_WhenPostDoesNotExist()
    {
        // 1. Arrange
        var postId = Guid.NewGuid();
        var mockService = new Mock<IPostService>();
        mockService
            .Setup(s => s.GetPostByIdAsync(postId, null))
            .ReturnsAsync((PostDto?)null);

        var controller = new PostsController(mockService.Object);

        // 2. Act
        var result = await controller.GetPost(postId);

        // 3. Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetPost_ShouldReturnOk_WhenPostExists()
    {
        // 1. Arrange
        var postId = Guid.NewGuid();
        var postDto = new PostDto
        {
            Id = postId,
            Content = "Great winds at Burgas!"
        };

        var mockService = new Mock<IPostService>();
        mockService
            .Setup(s => s.GetPostByIdAsync(postId, null))
            .ReturnsAsync(postDto);

        var controller = new PostsController(mockService.Object);

        // 2. Act
        var result = await controller.GetPost(postId);

        // 3. Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedPost = Assert.IsType<PostDto>(okResult.Value);
        Assert.Equal(postId, returnedPost.Id);
    }

    [Fact]
    public async Task CreatePost_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        // 1. Arrange
        var mockService = new Mock<IPostService>();
        var controller = new PostsController(mockService.Object);
        var dto = new CreatePostDto { Content = "No user test" };

        // 2. Act
        var result = await controller.CreatePost(dto);

        // 3. Assert
        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task CreatePost_ShouldReturnBadRequest_WhenServiceReturnsError()
    {
        // 1. Arrange
        var userId = Guid.NewGuid();
        var dto = new CreatePostDto { Content = "Test", SpotId = Guid.NewGuid() };

        var mockService = new Mock<IPostService>();
        mockService
            .Setup(s => s.CreatePostAsync(dto, userId))
            .ReturnsAsync(((PostDto?)null, "Specified spot does not exist."));

        var controller = new PostsController(mockService.Object);
        ControllerTestHelper.SetLoggedUser(controller, userId);

        // 2. Act
        var result = await controller.CreatePost(dto);

        // 3. Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreatePost_ShouldReturnCreatedAtAction_WhenValid()
    {
        // 1. Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var dto = new CreatePostDto { Content = "Awesome day on the water!" };
        var createdPost = new PostDto { Id = postId, Content = dto.Content, UserId = userId };

        var mockService = new Mock<IPostService>();
        mockService
            .Setup(s => s.CreatePostAsync(dto, userId))
            .ReturnsAsync((createdPost, (string?)null));

        var controller = new PostsController(mockService.Object);
        ControllerTestHelper.SetLoggedUser(controller, userId);

        // 2. Act
        var result = await controller.CreatePost(dto);

        // 3. Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(PostsController.GetPost), createdResult.ActionName);
        Assert.Equal(postId, createdResult.RouteValues?["id"]);
    }

    [Fact]
    public async Task DeletePost_ShouldReturnForbid_WhenServiceReturnsForbidden()
    {
        // 1. Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var mockService = new Mock<IPostService>();
        mockService
            .Setup(s => s.DeletePostAsync(postId, userId))
            .ReturnsAsync(PostDeleteResult.Forbidden);

        var controller = new PostsController(mockService.Object);
        ControllerTestHelper.SetLoggedUser(controller, userId);

        // 2. Act
        var result = await controller.DeletePost(postId);

        // 3. Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task DeletePost_ShouldReturnNotFound_WhenServiceReturnsNotFound()
    {
        // 1. Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var mockService = new Mock<IPostService>();
        mockService
            .Setup(s => s.DeletePostAsync(postId, userId))
            .ReturnsAsync(PostDeleteResult.NotFound);

        var controller = new PostsController(mockService.Object);
        ControllerTestHelper.SetLoggedUser(controller, userId);

        // 2. Act
        var result = await controller.DeletePost(postId);

        // 3. Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeletePost_ShouldReturnNoContent_WhenServiceReturnsSuccess()
    {
        // 1. Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var mockService = new Mock<IPostService>();
        mockService
            .Setup(s => s.DeletePostAsync(postId, userId))
            .ReturnsAsync(PostDeleteResult.Success);

        var controller = new PostsController(mockService.Object);
        ControllerTestHelper.SetLoggedUser(controller, userId);

        // 2. Act
        var result = await controller.DeletePost(postId);

        // 3. Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task ToggleLike_ShouldReturnNotFound_WhenPostDoesNotExist()
    {
        // 1. Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var mockService = new Mock<IPostService>();
        mockService
            .Setup(s => s.ToggleLikeAsync(postId, userId))
            .ReturnsAsync((LikeResponseDto?)null);

        var controller = new PostsController(mockService.Object);
        ControllerTestHelper.SetLoggedUser(controller, userId);

        // 2. Act
        var result = await controller.ToggleLike(postId);

        // 3. Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task ToggleLike_ShouldReturnOk_WithLikeResponse()
    {
        // 1. Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var likeResponse = new LikeResponseDto { PostId = postId, IsLiked = true, LikesCount = 5 };

        var mockService = new Mock<IPostService>();
        mockService
            .Setup(s => s.ToggleLikeAsync(postId, userId))
            .ReturnsAsync(likeResponse);

        var controller = new PostsController(mockService.Object);
        ControllerTestHelper.SetLoggedUser(controller, userId);

        // 2. Act
        var result = await controller.ToggleLike(postId);

        // 3. Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<LikeResponseDto>(okResult.Value);
        Assert.True(response.IsLiked);
        Assert.Equal(5, response.LikesCount);
    }

    [Fact]
    public async Task GetComments_ShouldReturnNotFound_WhenPostDoesNotExist()
    {
        // 1. Arrange
        var postId = Guid.NewGuid();
        var mockService = new Mock<IPostService>();
        mockService
            .Setup(s => s.GetCommentsAsync(postId))
            .ReturnsAsync((IEnumerable<CommentDto>?)null);

        var controller = new PostsController(mockService.Object);

        // 2. Act
        var result = await controller.GetComments(postId);

        // 3. Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetComments_ShouldReturnOk_WhenPostExists()
    {
        // 1. Arrange
        var postId = Guid.NewGuid();
        var fakeComments = new List<CommentDto>
        {
            new() { Id = Guid.NewGuid(), PostId = postId, Content = "Epic!" }
        };

        var mockService = new Mock<IPostService>();
        mockService
            .Setup(s => s.GetCommentsAsync(postId))
            .ReturnsAsync(fakeComments);

        var controller = new PostsController(mockService.Object);

        // 2. Act
        var result = await controller.GetComments(postId);

        // 3. Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedComments = Assert.IsAssignableFrom<IEnumerable<CommentDto>>(okResult.Value);
        Assert.Single(returnedComments);
    }

    [Fact]
    public async Task AddComment_ShouldReturnCreatedAtAction_WhenValid()
    {
        // 1. Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var dto = new CreateCommentDto { Content = "Nice jump!" };
        var commentDto = new CommentDto { Id = Guid.NewGuid(), PostId = postId, UserId = userId, Content = dto.Content };

        var mockService = new Mock<IPostService>();
        mockService
            .Setup(s => s.AddCommentAsync(postId, dto, userId))
            .ReturnsAsync(commentDto);

        var controller = new PostsController(mockService.Object);
        ControllerTestHelper.SetLoggedUser(controller, userId);

        // 2. Act
        var result = await controller.AddComment(postId, dto);

        // 3. Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(PostsController.GetPost), createdResult.ActionName);
    }

    [Fact]
    public async Task DeleteComment_ShouldReturnNoContent_WhenSuccess()
    {
        // 1. Arrange
        var postId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var mockService = new Mock<IPostService>();
        mockService
            .Setup(s => s.DeleteCommentAsync(postId, commentId, userId))
            .ReturnsAsync(PostDeleteResult.Success);

        var controller = new PostsController(mockService.Object);
        ControllerTestHelper.SetLoggedUser(controller, userId);

        // 2. Act
        var result = await controller.DeleteComment(postId, commentId);

        // 3. Assert
        Assert.IsType<NoContentResult>(result);
    }
}
