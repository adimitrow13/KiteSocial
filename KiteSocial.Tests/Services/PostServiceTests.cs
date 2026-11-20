using KiteSocial.API.DTOs.Comments;
using KiteSocial.API.DTOs.Posts;
using KiteSocial.API.Models.Entities;
using KiteSocial.API.Services;
using KiteSocial.API.Services.Interfaces;
using KiteSocial.Tests.Common;
using Xunit;

namespace KiteSocial.Tests.Services;

public class PostServiceTests
{
    [Fact]
    public async Task ToggleLikeAsync_ShouldAddAndRemoveLike_WhenCalledTwice()
    {
        // 1. Arrange
        using var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        db.Posts.Add(new Post { Id = postId, UserId = userId, Content = "Great wind today!" });
        await db.SaveChangesAsync();

        var service = new PostService(db);

        // 2. Act & Assert 1 - First click adds like
        var firstClick = await service.ToggleLikeAsync(postId, userId);
        Assert.NotNull(firstClick);
        Assert.True(firstClick.IsLiked);
        Assert.Equal(1, firstClick.LikesCount);

        // 3. Act & Assert 2 - Second click removes like
        var secondClick = await service.ToggleLikeAsync(postId, userId);
        Assert.NotNull(secondClick);
        Assert.False(secondClick.IsLiked);
        Assert.Equal(0, secondClick.LikesCount);
    }

    [Fact]
    public async Task CreatePostAsync_ShouldReturnError_WhenReferencedSpotDoesNotExist()
    {
        // 1. Arrange
        using var db = TestDbContextFactory.Create();
        var service = new PostService(db);
        var dto = new CreatePostDto { Content = "Test", SpotId = Guid.NewGuid() };

        // 2. Act
        var (post, error) = await service.CreatePostAsync(dto, Guid.NewGuid());

        // 3. Assert
        Assert.Null(post);
        Assert.Equal("Referenced spot does not exist.", error);
    }

    [Fact]
    public async Task DeletePostAsync_ShouldReturnForbidden_WhenUserIsNotOwner()
    {
        // 1. Arrange
        using var db = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        var strangerId = Guid.NewGuid();
        var post = new Post { Id = Guid.NewGuid(), UserId = ownerId, Content = "My post" };
        db.Posts.Add(post);
        await db.SaveChangesAsync();

        var service = new PostService(db);

        // 2. Act
        var result = await service.DeletePostAsync(post.Id, strangerId);

        // 3. Assert
        Assert.Equal(PostDeleteResult.Forbidden, result);
    }

    [Fact]
    public async Task DeletePostAsync_ShouldReturnSuccess_WhenUserIsOwner()
    {
        // 1. Arrange
        using var db = TestDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        var post = new Post { Id = Guid.NewGuid(), UserId = ownerId, Content = "My post" };
        db.Posts.Add(post);
        await db.SaveChangesAsync();

        var service = new PostService(db);

        // 2. Act
        var result = await service.DeletePostAsync(post.Id, ownerId);

        // 3. Assert
        Assert.Equal(PostDeleteResult.Success, result);
        Assert.Null(await db.Posts.FindAsync(post.Id));
    }

    [Fact]
    public async Task AddCommentAsync_ShouldAddComment_WhenPostExists()
    {
        // 1. Arrange
        using var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        db.Posts.Add(new Post { Id = postId, UserId = userId, Content = "Nice session!" });
        await db.SaveChangesAsync();

        var service = new PostService(db);
        var dto = new CreateCommentDto { Content = "Epic jump!" };

        // 2. Act
        var comment = await service.AddCommentAsync(postId, dto, userId);

        // 3. Assert
        Assert.NotNull(comment);
        Assert.Equal("Epic jump!", comment.Content);
        Assert.Equal(postId, comment.PostId);
    }
}
