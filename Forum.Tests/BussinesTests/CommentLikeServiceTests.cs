using AutoMapper;
using Moq;
using WebApp.BusinessLogic.Models.CommentLikeModels;
using WebApp.BusinessLogic.Services;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Interfaces;
using Xunit;

namespace Forum.Tests.BussinesTests;
public class CommentLikeServiceTests
{
    private readonly Mock<ICommentLike> likeRepositoryMock;
    private readonly Mock<IMapper> mapperMock;
    private readonly CommentLikeService commentLikeService;

    public CommentLikeServiceTests()
    {
        this.likeRepositoryMock = new Mock<ICommentLike>();
        this.mapperMock = new Mock<IMapper>();
        this.commentLikeService = new CommentLikeService(this.likeRepositoryMock.Object, this.mapperMock.Object);
    }

    [Fact]
    public async Task AddAsyncShouldAddLikeWhenLikeDoesNotExist()
    {
        // Arrange
        var model = new CommentLikeModel { CommentId = 1, UserId = 1 };
        var like = new CommentLike { CommentId = 1, UserId = 1 };

        _ = this.likeRepositoryMock.Setup(r => r.GetLikeByCommentAndUserIdAsyn(model.CommentId, model.UserId))
            .ReturnsAsync((CommentLike)null!);
        _ = this.mapperMock.Setup(m => m.Map<CommentLike>(model)).Returns(like);
        _ = this.likeRepositoryMock.Setup(r => r.AddAsync(like)).Returns(Task.CompletedTask);
        _ = this.mapperMock.Setup(m => m.Map<CommentLikeModel>(like)).Returns(model);

        // Act
        var result = await this.commentLikeService.AddAsync(model);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(model.CommentId, result.CommentId);
        Assert.Equal(model.UserId, result.UserId);
        this.likeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<CommentLike>()), Times.Once);
    }

    [Fact]
    public async Task AddAsyncShouldReturnExistingLikeWhenLikeExists()
    {
        // Arrange
        var model = new CommentLikeModel { CommentId = 1, UserId = 1 };
        var existingLike = new CommentLike { CommentId = 1, UserId = 1 };

        _ = this.likeRepositoryMock.Setup(r => r.GetLikeByCommentAndUserIdAsyn(model.CommentId, model.UserId))
            .ReturnsAsync(existingLike);
        _ = this.mapperMock.Setup(m => m.Map<CommentLikeModel>(existingLike)).Returns(model);

        // Act
        var result = await this.commentLikeService.AddAsync(model);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(model.CommentId, result.CommentId);
        Assert.Equal(model.UserId, result.UserId);
        this.likeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<CommentLike>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsyncShouldThrowWhenLikeDoesNotExist()
    {
        // Arrange
        int userId = 1;
        int commentId = 1;

        _ = this.likeRepositoryMock.Setup(r => r.GetLikeByCommentAndUserIdAsyn(commentId, userId))
            .ReturnsAsync((CommentLike)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.commentLikeService.DeleteAsync(userId, commentId));
        Assert.Equal("Like not found.", exception.Message);
    }

    [Fact]
    public async Task DeleteAsyncShouldDeleteLikeWhenLikeExists()
    {
        // Arrange
        int userId = 1;
        int commentId = 1;
        var existingLike = new CommentLike { CommentId = commentId, UserId = userId };

        _ = this.likeRepositoryMock.Setup(r => r.GetLikeByCommentAndUserIdAsyn(commentId, userId))
            .ReturnsAsync(existingLike);

        // Act
        await this.commentLikeService.DeleteAsync(userId, commentId);

        // Assert
        this.likeRepositoryMock.Verify(r => r.Delete(existingLike), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsyncShouldReturnLikeWhenLikeExists()
    {
        // Arrange
        int userId = 1;
        int commentId = 1;
        var existingLike = new CommentLike { CommentId = commentId, UserId = userId };
        var expectedModel = new CommentLikeModel { CommentId = commentId, UserId = userId };

        _ = this.likeRepositoryMock.Setup(r => r.GetLikeByCommentAndUserIdAsyn(commentId, userId))
            .ReturnsAsync(existingLike);
        _ = this.mapperMock.Setup(m => m.Map<CommentLikeModel>(existingLike)).Returns(expectedModel);

        // Act
        var result = await this.commentLikeService.GetByIdAsync(userId, commentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedModel.CommentId, result.CommentId);
        Assert.Equal(expectedModel.UserId, result.UserId);
    }

    [Fact]
    public async Task GetByIdAsyncShouldReturnNullWhenLikeDoesNotExist()
    {
        // Arrange
        int userId = 1;
        int commentId = 1;

        _ = this.likeRepositoryMock.Setup(r => r.GetLikeByCommentAndUserIdAsyn(commentId, userId))
            .ReturnsAsync((CommentLike)null!);

        // Act
        var result = await this.commentLikeService.GetByIdAsync(userId, commentId);

        // Assert
        Assert.Null(result);
    }
}
