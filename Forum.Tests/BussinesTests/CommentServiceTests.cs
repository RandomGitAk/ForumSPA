using AutoMapper;
using Microsoft.AspNetCore.Http;
using Moq;
using WebApp.BusinessLogic.Models.CommentModels;
using WebApp.BusinessLogic.Models.UserModels;
using WebApp.BusinessLogic.Services;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Interfaces;
using Xunit;

namespace Forum.Tests.BussinesTests;
public class CommentServiceTests
{
    private readonly Mock<IComment> commentRepositoryMock;
    private readonly Mock<IMapper> mapperMock;
    private readonly Mock<IHttpContextAccessor> httpContextAccessorMock;
    private readonly Mock<HttpContext> httpContextMock;
    private readonly CommentService commentService;

    public CommentServiceTests()
    {
        this.commentRepositoryMock = new Mock<IComment>();
        this.mapperMock = new Mock<IMapper>();
        this.httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        this.httpContextMock = new Mock<HttpContext>();

        _ = this.httpContextAccessorMock.Setup(h => h.HttpContext).Returns(this.httpContextMock.Object);
        this.commentService = new CommentService(this.commentRepositoryMock.Object, this.mapperMock.Object, this.httpContextAccessorMock.Object);
    }

    [Fact]
    public async Task AddAsyncShouldAddCommentWhenValidModelProvided()
    {
        // Arrange
        var model = new CommentModel { Id = 1, Content = "Test comment" };
        var comment = new Comment { Id = 1, Content = "Test comment" };
        var expectedModel = new CommentWithUserModel { Id = 1, Content = "Test comment" };

        _ = this.mapperMock.Setup(m => m.Map<Comment>(model)).Returns(comment);
        _ = this.commentRepositoryMock.Setup(r => r.AddAsync(comment)).Returns(Task.CompletedTask);
        _ = this.mapperMock.Setup(m => m.Map<CommentWithUserModel>(comment)).Returns(expectedModel);

        // Act
        var result = await this.commentService.AddAsync(model);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedModel.Id, result.Id);
        Assert.Equal(expectedModel.Content, result.Content);
        this.commentRepositoryMock.Verify(r => r.AddAsync(comment), Times.Once);
    }

    [Fact]
    public async Task DeleteAsyncShouldThrowWhenCommentDoesNotExist()
    {
        // Arrange
        int modelId = 1;

        _ = this.commentRepositoryMock.Setup(r => r.GetByIdAsync(modelId)).ReturnsAsync((Comment)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.commentService.DeleteAsync(modelId));
        Assert.Equal("Comment not found.", exception.Message);
    }

    [Fact]
    public async Task DeleteAsyncShouldDeleteCommentWhenCommentExists()
    {
        // Arrange
        int modelId = 1;
        var existingComment = new Comment { Id = modelId };

        _ = this.commentRepositoryMock.Setup(r => r.GetByIdAsync(modelId)).ReturnsAsync(existingComment);
        _ = this.commentRepositoryMock.Setup(r => r.DeleteByIdAsync(modelId)).Returns(Task.CompletedTask);

        // Act
        await this.commentService.DeleteAsync(modelId);

        // Assert
        this.commentRepositoryMock.Verify(r => r.DeleteByIdAsync(modelId), Times.Once);
    }

    [Fact]
    public async Task GetAllAsyncShouldReturnAllComments()
    {
        // Arrange
        var comments = new List<Comment> { new Comment { Id = 1, Content = "Comment 1" } };
        var expectedModels = new List<CommentModel> { new CommentModel { Id = 1, Content = "Comment 1" } };

        _ = this.commentRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(comments);
        _ = this.mapperMock.Setup(m => m.Map<IEnumerable<CommentModel>>(comments)).Returns(expectedModels);

        // Act
        var result = await this.commentService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedModels.Count, result.Count());
    }

    [Fact]
    public async Task GetByIdAsyncShouldReturnCommentWhenCommentExists()
    {
        // Arrange
        int id = 1;
        var existingComment = new Comment { Id = id, Content = "Comment" };
        var expectedModel = new CommentModel { Id = id, Content = "Comment" };

        _ = this.commentRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingComment);
        _ = this.mapperMock.Setup(m => m.Map<CommentModel>(existingComment)).Returns(expectedModel);

        // Act
        var result = await this.commentService.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedModel.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsyncShouldReturnNullWhenCommentDoesNotExist()
    {
        // Arrange
        int id = 1;

        _ = this.commentRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Comment)null!);

        // Act
        var result = await this.commentService.GetByIdAsync(id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdWithUserAsyncShouldReturnCommentWithUserImageWhenCommentExists()
    {
        // Arrange
        int id = 1;
        var existingComment = new Comment
        {
            Id = id,
            Content = "Comment",
            User = new User { Image = "imagePath.jpg" }
        };
        var expectedModel = new CommentWithUserModel { Id = id, Content = "Comment", User = new UserModel { Image = "imagePath.jpg" } };

        _ = this.commentRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingComment);
        _ = this.mapperMock.Setup(m => m.Map<CommentWithUserModel>(existingComment)).Returns(expectedModel);
        _ = this.httpContextMock.Setup(req => req.Request.Scheme).Returns("http");
        _ = this.httpContextMock.Setup(req => req.Request.Host).Returns(new HostString("localhost:5000"));

        // Act
        var result = await this.commentService.GetByIdWithUserAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("http://localhost:5000/imagePath.jpg", result.User.Image);
    }

    [Fact]
    public async Task GetByPostIdWithUserAsyncShouldReturnCommentsWithUserImages()
    {
        // Arrange
        int postId = 1;
        var comments = new List<Comment>
        {
            new Comment
            {
                Id = 1,
                Content = "Comment 1",
                User = new User { Image = "imagePath1.jpg" },
                Replies = []
            }
        };
        var expectedModels = new List<CommentWithUserModel>
        {
            new CommentWithUserModel
            {
                Id = 1,
                Content = "Comment 1",
                User = new UserModel { Image = "imagePath1.jpg" }
            }
        };

        _ = this.commentRepositoryMock.Setup(r => r.GetByPostIdAsync(postId)).ReturnsAsync(comments);
        _ = this.mapperMock.Setup(m => m.Map<IEnumerable<CommentWithUserModel>>(comments)).Returns(expectedModels);
        _ = this.httpContextMock.Setup(req => req.Request.Scheme).Returns("http");
        _ = this.httpContextMock.Setup(req => req.Request.Host).Returns(new HostString("localhost:5000"));

        // Act
        var result = await this.commentService.GetByPostIdWithUserAsync(postId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("http://localhost:5000/imagePath1.jpg", result.First().User.Image);
    }

    [Fact]
    public async Task UpdateAsyncShouldThrowWhenCommentDoesNotExist()
    {
        // Arrange
        var model = new CommentModel { Id = 1 };

        _ = this.commentRepositoryMock.Setup(r => r.GetByIdAsync(model.Id)).ReturnsAsync((Comment)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.commentService.UpdateAsync(model));
        Assert.Equal("Comment not found.", exception.Message);
    }

    [Fact]
    public async Task UpdateAsyncShouldUpdateCommentWhenCommentExists()
    {
        // Arrange
        var model = new CommentModel { Id = 1, Content = "Updated Comment" };
        var existingComment = new Comment { Id = 1, Content = "Old Comment" };

        _ = this.commentRepositoryMock.Setup(r => r.GetByIdAsync(model.Id)).ReturnsAsync(existingComment);
        var updatedComment = new Comment { Id = model.Id, Content = model.Content };
        _ = this.mapperMock.Setup(m => m.Map<Comment>(model)).Returns(updatedComment);
        _ = this.commentRepositoryMock.Setup(r => r.UpdateAsync(updatedComment)).Returns(Task.CompletedTask);

        // Act
        await this.commentService.UpdateAsync(model);

        // Assert
        this.commentRepositoryMock.Verify(r => r.UpdateAsync(updatedComment), Times.Once);
    }
}
