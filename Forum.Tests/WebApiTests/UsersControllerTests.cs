using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using WebApp.BusinessLogic.Interfaces;
using WebApp.BusinessLogic.Models.UserModels;
using WebApp.WebApi.Controllers;
using WebApp.WebApi.ViewModels;
using Xunit;

namespace Forum.Tests.WebApiTests;
public class UsersControllerTests
{
    private readonly Mock<IUserService> userServiceMock;
    private readonly UsersController usersController;
    private readonly Mock<ILogger<UsersController>> loggerMock;

    public UsersControllerTests()
    {
        this.userServiceMock = new Mock<IUserService>();
        this.loggerMock = new Mock<ILogger<UsersController>>();
        this.usersController = new UsersController(this.userServiceMock.Object, this.loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdShouldReturnOkWhenUserExists()
    {
        // Arrange
        int userId = 1;
        var user = new UserModel { Id = userId, FirstName = "John", LastName = "Doe" };
        _ = this.userServiceMock.Setup(s => s.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await this.usersController.GetById(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(user, okResult.Value);
    }

    [Fact]
    public async Task GetByIdShouldReturnNotFoundWhenUserDoesNotExist()
    {
        // Arrange
        int userId = 1;
        _ = this.userServiceMock.Setup(s => s.GetByIdAsync(userId)).ReturnsAsync((UserModel)null!);

        // Act
        var result = await this.usersController.GetById(userId);

        // Assert
        _ = Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task PatchUserShouldReturnNoContentWhenUserUpdatedSuccessfully()
    {
        // Arrange
        int userId = 1;
        var updateUserRoleModel = new UpdateUserRoleModel { RoleId = 2 };
        var user = new UserModel { Id = userId };

        _ = this.userServiceMock.Setup(s => s.GetByIdAsync(userId)).ReturnsAsync(user);
        _ = this.userServiceMock.Setup(s => s.UpdateUserRoleAsync(userId, updateUserRoleModel.RoleId)).Returns(Task.CompletedTask);

        // Act
        var result = await this.usersController.PatchUser(userId, updateUserRoleModel);

        // Assert
        _ = Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task PatchUserShouldReturnNotFoundWhenUserDoesNotExist()
    {
        // Arrange
        int userId = 1;
        var updateUserRoleModel = new UpdateUserRoleModel { RoleId = 2 };

        _ = this.userServiceMock.Setup(s => s.GetByIdAsync(userId)).ReturnsAsync((UserModel)null!);

        // Act
        var result = await this.usersController.PatchUser(userId, updateUserRoleModel);

        // Assert
        _ = Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task RegisterShouldReturnOkWhenRegistrationIsSuccessful()
    {
        // Arrange
        var userModel = new UserRegisterModel { Email = "test@example.com", Password = "password" };
        _ = this.userServiceMock.Setup(s => s.RegisterUserAsync(userModel, "User")).ReturnsAsync(true);

        // Act
        var result = await this.usersController.Register(userModel);

        // Assert
        _ = Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task RegisterShouldReturnConflictWhenUserAlreadyExists()
    {
        // Arrange
        var userModel = new UserRegisterModel { Email = "test@example.com", Password = "password" };
        _ = this.userServiceMock.Setup(s => s.RegisterUserAsync(userModel, "User")).ReturnsAsync(false);

        // Act
        var result = await this.usersController.Register(userModel);

        // Assert
        _ = Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task UpdateUserShouldReturnNoContentWhenUserUpdatedSuccessfully()
    {
        // Arrange
        var userUpdateModel = new UserUpdateModel { FirstName = "Jane", LastName = "Doe" };
        int userId = 1;
        _ = this.usersController.ControllerContext.HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) })) };

        _ = this.userServiceMock.Setup(s => s.UpdateAsync(It.IsAny<UserModel>())).Returns(Task.CompletedTask);

        // Act
        var result = await this.usersController.UpdateUser(userUpdateModel);

        // Assert
        _ = Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateUserShouldReturnUnauthorizedWhenUserIdNotFound()
    {
        // Arrange
        var userUpdateModel = new UserUpdateModel();
        this.usersController.ControllerContext.HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "0") })) };

        // Act
        var result = await this.usersController.UpdateUser(userUpdateModel);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("User id not found.", unauthorizedResult.Value);
    }

    [Fact]
    public async Task DeleteShouldReturnNoContentWhenUserDeletedSuccessfully()
    {
        // Arrange
        int userId = 1;
        _ = this.userServiceMock.Setup(s => s.DeleteAsync(userId)).Returns(Task.CompletedTask);

        // Act
        var result = await this.usersController.Delete(userId);

        // Assert
        _ = Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteShouldReturnNotFoundWhenUserDoesNotExist()
    {
        // Arrange
        int userId = 1;
        _ = this.userServiceMock.Setup(s => s.DeleteAsync(userId)).ThrowsAsync(new InvalidOperationException());

        // Act
        var result = await this.usersController.Delete(userId);

        // Assert
        _ = Assert.IsType<NotFoundResult>(result);
    }
}
