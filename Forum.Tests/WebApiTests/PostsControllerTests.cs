using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using WebApp.BusinessLogic.Interfaces;
using WebApp.BusinessLogic.Models.PostModels;
using WebApp.BusinessLogic.Models;
using WebApp.WebApi.Controllers;
using WebApp.WebApi.ViewModels;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Forum.Tests.WebApiTests;
public class PostsControllerTests
{
    private readonly Mock<IPostService> postServiceMock;
    private readonly PostsController postsController;
    private readonly Mock<ILogger<PostsController>> loggerMock;

    public PostsControllerTests()
    {
        this.postServiceMock = new Mock<IPostService>();
        this.loggerMock = new Mock<ILogger<PostsController>>();
        this.postsController = new PostsController(this.postServiceMock.Object, this.loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdShouldReturnOkWhenPostExists()
    {
        // Arrange
        int postId = 1;
        var postDetails = new PostWithDetailsModel { Id = postId, Title = "Post Details" };
        _ = this.postServiceMock.Setup(service => service.GetPostWithDetailsByIdAsync(postId))
            .ReturnsAsync(postDetails);

        // Act
        var result = await this.postsController.GetById(postId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedPost = Assert.IsType<PostWithDetailsModel>(okResult.Value);
        Assert.Equal(postId, returnedPost.Id);
        Assert.Equal("Post Details", returnedPost.Title);
    }

    [Fact]
    public async Task GetByIdShouldReturnNotFoundWhenPostDoesNotExist()
    {
        // Arrange
        int postId = 1;
        _ = this.postServiceMock.Setup(service => service.GetPostWithDetailsByIdAsync(postId))
            .ReturnsAsync((PostWithDetailsModel)null!);

        // Act
        var result = await this.postsController.GetById(postId);

        // Assert
        _ = Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void GetUserPostsShouldReturnOkWithUserPosts()
    {
        // Arrange
        var paginationParams = new PaginationParamsFromQueryModel { Page = 1, PerPage = 10 };
        int userId = 1;
        var mockUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "mock"));

        this.postsController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = mockUser }
        };
        var postModelPagination = new PostModelPagination
        {
            Items = [new PostModel { Id = 1, Title = "User Post" }]
        };

        _ = this.postServiceMock.Setup(service => service.GetPostsByUserId(paginationParams, userId))
            .Returns(postModelPagination);

        // Act
        var result = this.postsController.GetUserPosts(paginationParams);

        // Assert
        _ = Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task PostShouldReturnCreatedAtActionWhenPostCreated()
    {
        // Arrange
        var inputModel = new PostInputAddModel { Title = "New Post", Content = "Content", CategoryId = 1 };
        int userId = 1;
        var mockUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "mock"));

        this.postsController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = mockUser }
        };

        var createdPost = new PostWithDetailsModel { Id = 1, Title = "New Post", Content = "Content" };
        _ = this.postServiceMock.Setup(service => service.AddAsync(It.IsAny<CreatePostModel>()))
            .ReturnsAsync(new PostWithDetailsModel { Id = 1 });

        _ = this.postServiceMock.Setup(service => service.GetPostWithDetailsByIdAsync(1))
            .ReturnsAsync(createdPost);

        // Act
        var result = await this.postsController.Post(inputModel);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(this.postsController.GetById), createdAtActionResult.ActionName);
        Assert.Equal(1, createdAtActionResult.RouteValues!["id"]);
        Assert.Equal(createdPost, createdAtActionResult.Value);
    }

    [Fact]
    public async Task DeleteShouldReturnNoContentWhenPostDeleted()
    {
        // Arrange
        int postId = 1;
        _ = this.postServiceMock.Setup(service => service.DeleteAsync(postId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await this.postsController.Delete(postId);

        // Assert
        _ = Assert.IsType<NoContentResult>(result);
    }
}
