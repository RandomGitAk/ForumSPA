using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using WebApp.BusinessLogic.Interfaces;
using WebApp.BusinessLogic.Models.CommentModels;
using WebApp.WebApi.Controllers;
using WebApp.WebApi.ViewModels;
using Xunit;

namespace Forum.Tests.WebApiTests;
public class CommentsControllerTests
{
    private readonly Mock<ICommentService> commentServiceMock;
    private readonly CommentsController commentsController;
    private readonly Mock<ILogger<CommentsController>> loggerMock;

    public CommentsControllerTests()
    {
        this.commentServiceMock = new Mock<ICommentService>();
        this.loggerMock = new Mock<ILogger<CommentsController>>();
        this.commentsController = new CommentsController(this.commentServiceMock.Object, this.loggerMock.Object);
    }

    [Fact]
    public async Task GetByPostIdShouldReturnOkResultWithComments()
    {
        // Arrange
        int postId = 1;
        var comments = new List<CommentWithUserModel>
        {
            new CommentWithUserModel { Id = 1, Content = "Test Comment" }
        };

        _ = this.commentServiceMock.Setup(service => service.GetByPostIdWithUserAsync(postId))
                                   .ReturnsAsync(comments);

        // Act
        var result = await this.commentsController.GetByPostId(postId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedComments = Assert.IsAssignableFrom<IEnumerable<CommentWithUserModel>>(okResult.Value);
        _ = Assert.Single(returnedComments);
    }

    [Fact]
    public async Task GetByPostIdShouldReturnNotFoundWhenCommentsDoNotExist()
    {
        // Arrange
        int postId = 1;

        _ = this.commentServiceMock.Setup(service => service.GetByPostIdWithUserAsync(postId))
                                   .ReturnsAsync((IEnumerable<CommentWithUserModel>)null!);

        // Act
        var result = await this.commentsController.GetByPostId(postId);

        // Assert
        _ = Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetByIdShouldReturnOkResultWithComment()
    {
        // Arrange
        int commentId = 1;
        var comment = new CommentModel { Id = 1, Content = "Test Comment" };

        _ = this.commentServiceMock.Setup(service => service.GetByIdAsync(commentId))
                                   .ReturnsAsync(comment);

        // Act
        var result = await this.commentsController.GetById(commentId);

        // Assert
        _ = Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetByIdShouldReturnNotFoundWhenCommentDoesNotExist()
    {
        // Arrange
        int commentId = 1;

        _ = this.commentServiceMock.Setup(service => service.GetByIdAsync(commentId))
                                   .ReturnsAsync((CommentModel)null!);

        // Act
        var result = await this.commentsController.GetById(commentId);

        // Assert
        _ = Assert.IsType<NotFoundResult>(result.Result);
    }


    [Fact]
    public async Task PostShouldReturnCreatedAtActionWhenValidModel()
    {
        // Arrange
        var inputModel = new CommentInputAddModel
        {
            Content = "Valid content",
            PostId = 1,
            ParentCommentId = null
        };

        var createdComment = new CommentModel { Id = 1, Content = inputModel.Content, PostId = inputModel.PostId, UserId = 1 };
        var createdCommentWithUser = new CommentWithUserModel { Id = createdComment.Id, Content = createdComment.Content };

        var mockUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
        new Claim(ClaimTypes.NameIdentifier, "1")
        }, "mock"));

        this.commentsController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = mockUser
            }
        };

        // Мок commentService для AddAsync
        _ = this.commentServiceMock.Setup(service => service.AddAsync(It.IsAny<CommentModel>()))
            .ReturnsAsync(createdCommentWithUser);

        // Мок commentService для GetByIdWithUserAsync
        _ = this.commentServiceMock.Setup(service => service.GetByIdWithUserAsync(createdComment.Id))
            .ReturnsAsync(createdCommentWithUser);

        // Act
        var result = await this.commentsController.Post(inputModel);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(this.commentsController.GetById), createdAtActionResult.ActionName);
        Assert.Equal(createdCommentWithUser.Id, createdAtActionResult.RouteValues!["id"]);
        Assert.Equal(createdCommentWithUser, createdAtActionResult.Value);
    }

    [Fact]
    public async Task PostShouldReturnBadRequestWhenModelStateIsInvalid()
    {
        // Arrange

        this.commentsController.ModelState.AddModelError("Content", "Required");

        // Mocking the user claims
        var userIdClaimValue = "1"; // Mock user id as a string
        var mockUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
        new Claim(ClaimTypes.NameIdentifier, userIdClaimValue)
        }, "mock"));

        this.commentsController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = mockUser
            }
        };

        // Act
        var result = await this.commentsController.Post(null!);

        // Assert
        _ = Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task PostShouldReturnUnauthorizedWhenUserIdIsZero()
    {
        // Arrange
        var inputModel = new CommentInputAddModel();
        this.commentsController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            }
        };

        // Act
        var result = await this.commentsController.Post(inputModel);

        // Assert
        _ = Assert.IsType<UnauthorizedObjectResult>(result);
    }
}
