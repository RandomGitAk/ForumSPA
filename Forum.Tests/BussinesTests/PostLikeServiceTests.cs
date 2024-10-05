using AutoMapper;
using Moq;
using WebApp.BusinessLogic.Models.PostLikeModels;
using WebApp.BusinessLogic.Services;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Interfaces;
using Xunit;

namespace Forum.Tests.BussinesTests;
public class PostLikeServiceTests
{
    private readonly Mock<IPostLike> likeRepositoryMock;
    private readonly Mock<IMapper> mapperMock;
    private readonly PostLikeService postLikeService;

    public PostLikeServiceTests()
    {
        this.likeRepositoryMock = new Mock<IPostLike>();
        this.mapperMock = new Mock<IMapper>();
        this.postLikeService = new PostLikeService(this.likeRepositoryMock.Object, this.mapperMock.Object);
    }

    [Fact]
    public async Task AddAsyncShouldUpdateLikeWhenLikeExists()
    {
        // Arrange
        var model = new PostLikeModel { PostId = 1, UserId = 1, IsLike = true };
        var existingLike = new PostLike { PostId = 1, UserId = 1, IsLike = false };

        _ = this.likeRepositoryMock.Setup(r => r.GetLikeByPostAndUserIdAsyn(model.PostId, model.UserId))
            .ReturnsAsync(existingLike);
        existingLike.IsLike = model.IsLike;
        _ = this.likeRepositoryMock.Setup(r => r.UpdateAsync(existingLike)).Returns(Task.CompletedTask);
        _ = this.mapperMock.Setup(m => m.Map<PostLikeModel>(existingLike)).Returns(model);

        // Act
        var result = await this.postLikeService.AddAsync(model);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(model.PostId, result.PostId);
        Assert.Equal(model.UserId, result.UserId);
        Assert.Equal(model.IsLike, result.IsLike);
        this.likeRepositoryMock.Verify(r => r.UpdateAsync(existingLike), Times.Once);
        this.likeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<PostLike>()), Times.Never);
    }

    [Fact]
    public async Task AddAsyncShouldAddLikeWhenLikeDoesNotExist()
    {
        // Arrange
        var model = new PostLikeModel { PostId = 1, UserId = 1, IsLike = true };
        var newLike = new PostLike { PostId = 1, UserId = 1, IsLike = true };

        _ = this.likeRepositoryMock.Setup(r => r.GetLikeByPostAndUserIdAsyn(model.PostId, model.UserId))
            .ReturnsAsync((PostLike)null!);
        _ = this.mapperMock.Setup(m => m.Map<PostLike>(model)).Returns(newLike);
        _ = this.likeRepositoryMock.Setup(r => r.AddAsync(newLike)).Returns(Task.CompletedTask);
        _ = this.mapperMock.Setup(m => m.Map<PostLikeModel>(newLike)).Returns(model);

        // Act
        var result = await this.postLikeService.AddAsync(model);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(model.PostId, result.PostId);
        Assert.Equal(model.UserId, result.UserId);
        Assert.Equal(model.IsLike, result.IsLike);
        this.likeRepositoryMock.Verify(r => r.AddAsync(newLike), Times.Once);
        this.likeRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<PostLike>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsyncShouldThrowWhenLikeDoesNotExist()
    {
        // Arrange
        int userId = 1;
        int postId = 1;

        _ = this.likeRepositoryMock.Setup(r => r.GetLikeByPostAndUserIdAsyn(postId, userId))
            .ReturnsAsync((PostLike)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.postLikeService.DeleteAsync(userId, postId));
        Assert.Equal("Like not found.", exception.Message);
    }

    [Fact]
    public async Task DeleteAsyncShouldDeleteLikeWhenLikeExists()
    {
        // Arrange
        int userId = 1;
        int postId = 1;
        var existingLike = new PostLike { PostId = postId, UserId = userId };

        _ = this.likeRepositoryMock.Setup(r => r.GetLikeByPostAndUserIdAsyn(postId, userId))
            .ReturnsAsync(existingLike);

        // Act
        await this.postLikeService.DeleteAsync(userId, postId);

        // Assert
        this.likeRepositoryMock.Verify(r => r.Delete(existingLike), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsyncShouldReturnLikeWhenLikeExists()
    {
        // Arrange
        int userId = 1;
        int postId = 1;
        var existingLike = new PostLike { PostId = postId, UserId = userId };
        var expectedModel = new PostLikeModel { PostId = postId, UserId = userId, IsLike = true };

        _ = this.likeRepositoryMock.Setup(r => r.GetLikeByPostAndUserIdAsyn(postId, userId))
            .ReturnsAsync(existingLike);
        _ = this.mapperMock.Setup(m => m.Map<PostLikeModel>(existingLike)).Returns(expectedModel);

        // Act
        var result = await this.postLikeService.GetByIdAsync(userId, postId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedModel.PostId, result.PostId);
        Assert.Equal(expectedModel.UserId, result.UserId);
        Assert.Equal(expectedModel.IsLike, result.IsLike);
    }

    [Fact]
    public async Task GetByIdAsyncShouldReturnNullWhenLikeDoesNotExist()
    {
        // Arrange
        int userId = 1;
        int postId = 1;

        _ = this.likeRepositoryMock.Setup(r => r.GetLikeByPostAndUserIdAsyn(postId, userId))
            .ReturnsAsync((PostLike)null!);

        // Act
        var result = await this.postLikeService.GetByIdAsync(userId, postId);

        // Assert
        Assert.Null(result);
    }
}
