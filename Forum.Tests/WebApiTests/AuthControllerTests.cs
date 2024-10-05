using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using WebApp.BusinessLogic.Interfaces;
using WebApp.BusinessLogic.Models.UserModels;
using WebApp.BusinessLogic.Models;
using WebApp.DataAccess.Entities;
using WebApp.WebApi.Controllers;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Forum.Tests.WebApiTests;
public class AuthControllerTests
{
    private readonly Mock<IUserService> userServiceMock;
    private readonly AuthController authController;
    private readonly Mock<ILogger<AuthController>> loggerMock;

    public AuthControllerTests()
    {
        this.userServiceMock = new Mock<IUserService>();
        this.loggerMock = new Mock<ILogger<AuthController>>();
        this.authController = new AuthController(this.userServiceMock.Object, this.loggerMock.Object);
    }

    [Fact]
    public async Task LoginShouldReturnOkWhenModelIsValid()
    {
        // Arrange
        var model = new UserLoginModel { Email = "test@example.com", Password = "password" };
        var tokensResponse = new TokenResponseModel { AccesToken = "access_token", RefreshToken = "refresh_token" };

        _ = this.userServiceMock.Setup(s => s.LoginAsync(model)).ReturnsAsync(tokensResponse);

        // Act
        var result = await this.authController.Login(model);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(tokensResponse, okResult.Value);
    }

    [Fact]
    public async Task LoginShouldReturnBadRequestWhenModelIsInvalid()
    {
        // Arrange
        this.authController.ModelState.AddModelError("Email", "Required");

        // Act
        var result = await this.authController.Login(new UserLoginModel());

        // Assert
        _ = Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task LoginShouldReturnBadRequestWhenArgumentExceptionThrown()
    {
        // Arrange
        var model = new UserLoginModel { Email = "test@example.com", Password = "password" };
        _ = this.userServiceMock.Setup(s => s.LoginAsync(model)).ThrowsAsync(new ArgumentException("Invalid login attempt."));

        // Act
        var result = await this.authController.Login(model);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid login attempt.", badRequestResult.Value);
    }

    [Fact]
    public async Task LogoutShouldReturnOkWhenUserIdIsFound()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

        this.authController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

        // Act
        var result = await this.authController.Logout();

        // Assert
        _ = Assert.IsType<OkObjectResult>(result);
        this.userServiceMock.Verify(s => s.DeleteRefreshTokenAsync(1), Times.Once);
    }

    [Fact]
    public async Task LogoutShouldReturnUnauthorizedWhenUserIdIsNotFound()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "0") };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

        this.authController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

        // Act
        var result = await this.authController.Logout();

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("User id not found.", unauthorizedResult.Value);
    }

    [Fact]
    public async Task RefreshTokenAsyncShouldReturnOkWhenRefreshTokenIsValid()
    {
        // Arrange
        var request = new RefreshTokenRequest { RefreshToken = "valid_refresh_token" };
        var user = new User { Id = 1 };
        var userModel = new UserModel { Id = 1 };
        var tokensResponse = new TokenResponseModel { AccesToken = "new_access_token", RefreshToken = "new_refresh_token" };

        _ = this.userServiceMock.Setup(s => s.GetUserByRefreshTokenAsync(request.RefreshToken)).ReturnsAsync(userModel);
        _ = this.userServiceMock.Setup(s => s.RefreshTokenAsync(user.Id, request.RefreshToken)).ReturnsAsync(tokensResponse);

        // Act
        var result = await this.authController.RefreshTokenAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(tokensResponse, okResult.Value);
    }

    [Fact]
    public async Task RefreshTokenAsyncShouldReturnUnauthorizedWhenUnauthorizedAccessExceptionThrown()
    {
        // Arrange
        var request = new RefreshTokenRequest { RefreshToken = "invalid_refresh_token" };
        _ = this.userServiceMock.Setup(s => s.GetUserByRefreshTokenAsync(request.RefreshToken)).ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var result = await this.authController.RefreshTokenAsync(request);

        // Assert
        _ = Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetCurrentUserShouldReturnOkWhenUserExists()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var userModel = new UserModel { Id = 1, Email = "test@example.com" };

        this.authController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        _ = this.userServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(userModel);

        // Act
        var result = await this.authController.GetCurrentUser();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(userModel, okResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserShouldReturnUnauthorizedWhenUserIdIsNotFound()
    {
        // Arrange
        this.authController.ControllerContext.HttpContext = new DefaultHttpContext { User = null! };

        // Act
        var result = await this.authController.GetCurrentUser();

        // Assert
        _ = Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task GetCurrentUserShouldReturnNotFoundWhenUserDoesNotExist()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
        this.authController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

        _ = this.userServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((UserModel)null!);

        // Act
        var result = await this.authController.GetCurrentUser();

        // Assert
        _ = Assert.IsType<NotFoundResult>(result.Result);
    }
}
