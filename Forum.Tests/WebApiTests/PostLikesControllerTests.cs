using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using WebApp.BusinessLogic.Interfaces;
using WebApp.BusinessLogic.Models.PostLikeModels;
using WebApp.WebApi.Controllers;
using Xunit;

namespace Forum.Tests.WebApiTests;
public class PostLikesControllerTests
{
    private readonly Mock<IPostLikeService> likeServiceMock;
    private readonly PostLikesController postLikesController;
    private readonly Mock<ILogger<PostLikesController>> loggerMock;

    public PostLikesControllerTests()
    {
        this.likeServiceMock = new Mock<IPostLikeService>();
        this.loggerMock = new Mock<ILogger<PostLikesController>>();
        this.postLikesController = new PostLikesController(this.likeServiceMock.Object, this.loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdShouldReturnOkResultWithLike()
    {
        // Arrange
        int postId = 1;
        int userId = 1;
        var mockUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "mock"));

        this.postLikesController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = mockUser }
        };

        var postLike = new PostLikeModel { PostId = postId, UserId = userId, IsLike = true };
        _ = this.likeServiceMock.Setup(service => service.GetByIdAsync(userId, postId))
                            .ReturnsAsync(postLike);

        // Act
        var result = await this.postLikesController.GetById(postId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedLike = Assert.IsType<PostLikeModel>(okResult.Value);
        Assert.Equal(postLike, returnedLike);
    }

    [Fact]
    public async Task GetByIdShouldReturnNotFoundWhenLikeDoesNotExist()
    {
        // Arrange
        int postId = 1;
        int userId = 1;

        var mockUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "mock"));

        this.postLikesController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = mockUser }
        };

        _ = this.likeServiceMock.Setup(service => service.GetByIdAsync(userId, postId))
                            .ReturnsAsync((PostLikeModel)null!);

        // Act
        var result = await this.postLikesController.GetById(postId);

        // Assert
        _ = Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task PostShouldReturnCreatedAtActionWhenLikeAdded()
    {
        // Arrange
        var inputModel = new LikeValuePost { PostId = 1, IsLike = true };
        int userId = 1;

        var mockUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "mock"));

        this.postLikesController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = mockUser }
        };

        var addedLike = new PostLikeModel { PostId = inputModel.PostId, UserId = userId, IsLike = inputModel.IsLike };

        _ = this.likeServiceMock.Setup(service => service.AddAsync(It.IsAny<PostLikeModel>()))
                            .ReturnsAsync(addedLike);

        _ = this.likeServiceMock.Setup(service => service.GetByIdAsync(userId, inputModel.PostId))
                            .ReturnsAsync(addedLike);

        // Act
        var result = await this.postLikesController.Post(inputModel);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(this.postLikesController.GetById), createdAtActionResult.ActionName);
        Assert.Equal(inputModel.PostId, createdAtActionResult.RouteValues!["id"]);
    }

    [Fact]
    public async Task DeleteShouldReturnNoContentWhenLikeDeleted()
    {
        // Arrange
        int postId = 1;
        int userId = 1;

        var mockUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "mock"));

        this.postLikesController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = mockUser }
        };

        _ = this.likeServiceMock.Setup(service => service.DeleteAsync(userId, postId))
                            .Returns(Task.CompletedTask);

        // Act
        var result = await this.postLikesController.Delete(postId);

        // Assert
        _ = Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteShouldReturnNotFoundWhenLikeDoesNotExist()
    {
        // Arrange
        int postId = 1;
        int userId = 1;

        var mockUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "mock"));

        this.postLikesController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = mockUser }
        };

        _ = this.likeServiceMock.Setup(service => service.DeleteAsync(userId, postId))
                            .Throws(new InvalidOperationException());

        // Act
        var result = await this.postLikesController.Delete(postId);

        // Assert
        _ = Assert.IsType<NotFoundResult>(result);
    }
}
