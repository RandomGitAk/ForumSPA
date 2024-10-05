using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using WebApp.BusinessLogic.Interfaces;
using WebApp.BusinessLogic.Models.CommentLikeModels;
using WebApp.WebApi.Controllers;
using WebApp.WebApi.ViewModels;
using Xunit;

namespace Forum.Tests.WebApiTests;
public class CommentLikesControllerTests
{
    private readonly Mock<ICommentLikeService> likeServiceMock;
    private readonly CommentLikesController commentLikesController;
    private readonly Mock<ILogger<CommentLikesController>> loggerMock;

    public CommentLikesControllerTests()
    {
        this.likeServiceMock = new Mock<ICommentLikeService>();
        this.loggerMock = new Mock<ILogger<CommentLikesController>>();
        this.commentLikesController = new CommentLikesController(this.likeServiceMock.Object, this.loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdShouldReturnOkResultWithLike()
    {
        // Arrange
        int userId = 1;
        int commentId = 1;
        var likeModel = new CommentLikeModel { CommentId = commentId, UserId = userId };

        _ = this.likeServiceMock.Setup(service => service.GetByIdAsync(userId, commentId)).ReturnsAsync(likeModel);
        this.commentLikesController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                }))
            }
        };

        // Act
        var result = await this.commentLikesController.GetById(commentId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedLike = Assert.IsType<CommentLikeModel>(okResult.Value);
        Assert.Equal(commentId, returnedLike.CommentId);
    }

    [Fact]
    public async Task GetByIdShouldReturnNotFoundWhenLikeDoesNotExist()
    {
        // Arrange
        int userId = 1;
        int likeId = 1;

        _ = this.likeServiceMock.Setup(service => service.GetByIdAsync(userId, likeId)).ReturnsAsync((CommentLikeModel)null!);
        this.commentLikesController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                }))
            }
        };

        // Act
        var result = await this.commentLikesController.GetById(likeId);

        // Assert
        _ = Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task PostShouldReturnCreatedResultWithLike()
    {
        // Arrange
        int userId = 1;
        var model = new CommentLikeValue { CommentId = 1 };
        var likeModel = new CommentLikeModel { CommentId = 1, UserId = userId };

        _ = this.likeServiceMock.Setup(service => service.AddAsync(It.IsAny<CommentLikeModel>())).ReturnsAsync(likeModel);
        this.commentLikesController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                }))
            }
        };

        // Act
        var result = await this.commentLikesController.Post(model);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(this.commentLikesController.GetById), createdResult.ActionName);
        Assert.Equal(likeModel.CommentId, createdResult!.RouteValues!["id"]);
    }

    [Fact]
    public async Task PostShouldReturnBadRequestWhenModelStateIsInvalid()
    {
        // Arrange
        var mockUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "mock"));

        this.commentLikesController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = mockUser
            }
        };

        this.commentLikesController.ModelState.AddModelError("CommentId", "Required");

        // Act
        var result = await this.commentLikesController.Post(new CommentLikeValue());

        // Assert
        _ = Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteShouldReturnNoContentWhenLikeExists()
    {
        // Arrange
        int userId = 1;
        int likeId = 1;

        _ = this.likeServiceMock.Setup(service => service.DeleteAsync(userId, likeId)).Returns(Task.CompletedTask);
        this.commentLikesController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                }))
            }
        };

        // Act
        var result = await this.commentLikesController.Delete(likeId);

        // Assert
        _ = Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteShouldReturnNotFoundWhenLikeDoesNotExist()
    {
        // Arrange
        int userId = 1;
        int likeId = 1;

        _ = this.likeServiceMock.Setup(service => service.DeleteAsync(userId, likeId)).Throws(new InvalidOperationException());
        this.commentLikesController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                }))
            }
        };

        // Act
        var result = await this.commentLikesController.Delete(likeId);

        // Assert
        _ = Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteShouldReturnUnauthorizedWhenUserIdIsZero()
    {
        // Arrange
        this.commentLikesController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            }
        };

        // Act
        var result = await this.commentLikesController.Delete(1);

        // Assert
        _ = Assert.IsType<UnauthorizedObjectResult>(result);
    }
}
