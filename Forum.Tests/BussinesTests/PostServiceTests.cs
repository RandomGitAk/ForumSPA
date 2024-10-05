using AutoMapper;
using Microsoft.AspNetCore.Http;
using Moq;
using WebApp.BusinessLogic.Models.PostModels;
using WebApp.BusinessLogic.Services;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Interfaces;
using Xunit;

namespace Forum.Tests.BussinesTests;
public class PostServiceTests
{
    private readonly Mock<IPost> postRepositoryMock;
    private readonly Mock<IHttpContextAccessor> httpContextAccessorMock;
    private readonly Mock<IMapper> mapperMock;
    private readonly PostService postService;

    public PostServiceTests()
    {
        this.postRepositoryMock = new Mock<IPost>();
        this.httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        this.mapperMock = new Mock<IMapper>();
        this.postService = new PostService(
            this.postRepositoryMock.Object,
            this.httpContextAccessorMock.Object,
            this.mapperMock.Object
        );
    }

    [Fact]
    public async Task AddAsyncShouldAddPost()
    {
        // Arrange
        var model = new CreatePostModel { Title = "New Post", Content = "Content" };
        var post = new Post { Id = 1, Title = "New Post", Content = "Content" };

        _ = this.mapperMock.Setup(m => m.Map<Post>(model)).Returns(post);
        _ = this.postRepositoryMock.Setup(r => r.AddAsync(post)).Returns(Task.CompletedTask);
        _ = this.mapperMock.Setup(m => m.Map<PostModel>(post)).Returns(new PostModel { Id = 1 });

        // Act
        var result = await this.postService.AddAsync(model);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(post.Id, result.Id);
        this.postRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Post>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsyncShouldThrowWhenPostDoesNotExist()
    {
        // Arrange
        int postId = 1;

        _ = this.postRepositoryMock.Setup(r => r.GetByIdAsync(postId)).ReturnsAsync((Post)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.postService.DeleteAsync(postId)
        );
        Assert.Equal("Post not found.", exception.Message);
    }

    [Fact]
    public async Task DeleteAsyncShouldDeletePostWhenExists()
    {
        // Arrange
        int postId = 1;
        var existingPost = new Post { Id = postId };

        _ = this.postRepositoryMock.Setup(r => r.GetByIdAsync(postId)).ReturnsAsync(existingPost);
        _ = this.postRepositoryMock.Setup(r => r.DeleteByIdAsync(postId)).Returns(Task.CompletedTask);

        // Act
        await this.postService.DeleteAsync(postId);

        // Assert
        this.postRepositoryMock.Verify(r => r.DeleteByIdAsync(postId), Times.Once);
    }

    [Fact]
    public async Task GetAllAsyncShouldReturnAllPosts()
    {
        // Arrange
        var posts = new List<Post>
        {
            new Post { Id = 1, Title = "Post 1" },
            new Post { Id = 2, Title = "Post 2" }
        };
        var postModels = new List<PostModel>
        {
            new PostModel { Id = 1, Title = "Post 1" },
            new PostModel { Id = 2, Title = "Post 2" }
        };

        _ = this.postRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(posts);
        _ = this.mapperMock.Setup(m => m.Map<IEnumerable<PostModel>>(posts)).Returns(postModels);

        // Act
        var result = await this.postService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Equal(postModels[0].Id, result.First().Id);
    }

    [Fact]
    public async Task GetByIdAsyncShouldReturnPostWhenExists()
    {
        // Arrange
        int postId = 1;
        var existingPost = new Post { Id = postId };
        var expectedModel = new PostModel { Id = postId };

        _ = this.postRepositoryMock.Setup(r => r.GetByIdAsync(postId)).ReturnsAsync(existingPost);
        _ = this.mapperMock.Setup(m => m.Map<PostModel>(existingPost)).Returns(expectedModel);

        // Act
        var result = await this.postService.GetByIdAsync(postId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedModel.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsyncShouldReturnNullWhenPostDoesNotExist()
    {
        // Arrange
        int postId = 1;

        _ = this.postRepositoryMock.Setup(r => r.GetByIdAsync(postId)).ReturnsAsync((Post)null!);

        // Act
        var result = await this.postService.GetByIdAsync(postId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsyncShouldThrowWhenPostDoesNotExist()
    {
        // Arrange
        var model = new CreatePostModel { Id = 1 };

        _ = this.postRepositoryMock.Setup(r => r.GetByIdAsync(model.Id)).ReturnsAsync((Post)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.postService.UpdateAsync(model)
        );
        Assert.Equal("Post not found.", exception.Message);
    }

    [Fact]
    public async Task UpdateAsyncShouldUpdatePostWhenExists()
    {
        // Arrange
        var model = new CreatePostModel { Id = 1, Title = "Updated Post" };
        var existingPost = new Post { Id = 1 };
        var updatedPost = new Post { Id = 1, Title = "Updated Post" };

        _ = this.postRepositoryMock.Setup(r => r.GetByIdAsync(model.Id)).ReturnsAsync(existingPost);
        _ = this.mapperMock.Setup(m => m.Map<Post>(model)).Returns(updatedPost);
        _ = this.postRepositoryMock.Setup(r => r.UpdateAsync(updatedPost)).Returns(Task.CompletedTask);

        // Act
        await this.postService.UpdateAsync(model);

        // Assert
        this.postRepositoryMock.Verify(r => r.UpdateAsync(updatedPost), Times.Once);
    }

    [Fact]
    public async Task IncrementPostViewsShouldThrowWhenPostDoesNotExist()
    {
        // Arrange
        int postId = 1;

        _ = this.postRepositoryMock.Setup(r => r.GetByIdAsync(postId)).ReturnsAsync((Post)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.postService.IncrementPostViews(postId)
        );
        Assert.Equal("Post not found.", exception.Message);
    }

    [Fact]
    public async Task IncrementPostViewsShouldIncrementViewsWhenPostExists()
    {
        // Arrange
        int postId = 1;
        var existingPost = new Post { Id = postId, Views = 5 };

        _ = this.postRepositoryMock.Setup(r => r.GetByIdAsync(postId)).ReturnsAsync(existingPost);
        _ = this.postRepositoryMock.Setup(r => r.UpdateAsync(existingPost)).Returns(Task.CompletedTask);

        // Act
        await this.postService.IncrementPostViews(postId);

        // Assert
        Assert.Equal(6, existingPost.Views);
        this.postRepositoryMock.Verify(r => r.UpdateAsync(existingPost), Times.Once);
    }
}
