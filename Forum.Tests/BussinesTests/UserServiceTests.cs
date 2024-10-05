using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using WebApp.BusinessLogic.Models.UserModels;
using WebApp.BusinessLogic.Services;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Interfaces;
using Xunit;

namespace Forum.Tests.BussinesTests;
public class UserServiceTests
{
    private readonly Mock<IUser> userRepositoryMock;
    private readonly Mock<IRole> roleRepositoryMock;
    private readonly Mock<IMapper> mapperMock;
    private readonly Mock<IWebHostEnvironment> appEnvironmentMock;
    private readonly Mock<IHttpContextAccessor> httpContextAccessorMock;
    private readonly UserService userService;

    public UserServiceTests()
    {
        this.userRepositoryMock = new Mock<IUser>();
        this.roleRepositoryMock = new Mock<IRole>();
        this.mapperMock = new Mock<IMapper>();
        this.appEnvironmentMock = new Mock<IWebHostEnvironment>();
        this.httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        this.userService = new UserService(
            this.userRepositoryMock.Object,
            this.roleRepositoryMock.Object,
            this.mapperMock.Object,
            this.appEnvironmentMock.Object,
            this.httpContextAccessorMock.Object);
    }

    [Fact]
    public async Task AddAsyncShouldAddUserAndReturnMappedUserModel()
    {
        // Arrange
        var userModel = new UserModel { FirstName = "John", LastName = "Doe", Email = "john@example.com" };
        var user = new User { FirstName = "John", LastName = "Doe", Email = "john@example.com" };

        _ = this.mapperMock.Setup(m => m.Map<User>(userModel)).Returns(user);
        _ = this.userRepositoryMock.Setup(r => r.AddAsync(user)).Returns(Task.CompletedTask);
        _ = this.mapperMock.Setup(m => m.Map<UserModel>(user)).Returns(userModel);

        // Act
        var result = await this.userService.AddAsync(userModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userModel.Email, result.Email);
        this.userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsyncShouldDeleteUserAndImageFileIfExists()
    {
        // Arrange
        int userId = 1;
        var user = new User { Id = userId, Image = "/userProfileImages/john.jpg" };
        _ = this.appEnvironmentMock.Setup(a => a.WebRootPath).Returns("C:\\path");
        _ = this.userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _ = this.userRepositoryMock.Setup(r => r.DeleteByIdAsync(userId)).Returns(Task.CompletedTask);

        // Act
        await this.userService.DeleteAsync(userId);

        // Assert
        this.userRepositoryMock.Verify(r => r.DeleteByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task LoginAsyncShouldReturnTokenResponseModelWhenUserExists()
    {
        // Arrange
        var userLoginModel = new UserLoginModel { Email = "john@example.com", Password = "password" };
        var user = new User { Email = "john@example.com", Salt = "salt", HashedPasssword = "hashedPassword", Role = new Role { Name = "user" } };

        _ = this.userRepositoryMock.Setup(r => r.IsUserExistsAsync(userLoginModel.Email)).ReturnsAsync(true);
        _ = this.userRepositoryMock.Setup(r => r.GetUserSaltAsync(userLoginModel.Email)).ReturnsAsync(user.Salt);
        _ = this.userRepositoryMock.Setup(r => r.VerifyUserAsync(It.IsAny<User>())).ReturnsAsync(true);
        _ = this.userRepositoryMock.Setup(r => r.GetUserByEmailAsync(userLoginModel.Email)).ReturnsAsync(user);

        // Act
        var result = await this.userService.LoginAsync(userLoginModel);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.AccesToken);
    }

    [Fact]
    public async Task RegisterUserAsyncShouldReturnFalseIfUserExists()
    {
        // Arrange
        var userRegisterModel = new UserRegisterModel { Email = "john@example.com", Password = "password" };

        _ = this.userRepositoryMock.Setup(r => r.IsUserExistsAsync(userRegisterModel.Email)).ReturnsAsync(true);

        // Act
        var result = await this.userService.RegisterUserAsync(userRegisterModel);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetByIdAsyncShouldReturnMappedUserModelWhenUserExists()
    {
        // Arrange
        int userId = 1;
        var user = new User { Id = userId, Email = "john@example.com" };
        var userModel = new UserModel { Id = userId, Email = "john@example.com" };

        _ = this.userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _ = this.mapperMock.Setup(m => m.Map<UserModel>(user)).Returns(userModel);
        _ = this.httpContextAccessorMock.Setup(h => h.HttpContext!.Request.Scheme).Returns("http");
        _ = this.httpContextAccessorMock.Setup(h => h.HttpContext!.Request.Host).Returns(new HostString("localhost"));

        // Act
        var result = await this.userService.GetByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userModel.Email, result.Email);
    }

    [Fact]
    public async Task UpdateAsyncShouldUpdateUserDetails()
    {
        // Arrange
        var userModel = new UserModel { Id = 1, FirstName = "Jane", LastName = "Doe" };
        var existingUser = new User { Id = 1, FirstName = "John", LastName = "Doe", Image = null! };

        _ = this.userRepositoryMock.Setup(r => r.GetByIdAsync(userModel.Id)).ReturnsAsync(existingUser);
        existingUser.FirstName = userModel.FirstName;
        existingUser.LastName = userModel.LastName;
        _ = this.userRepositoryMock.Setup(r => r.UpdateAsync(existingUser)).Returns(Task.CompletedTask);

        // Act
        await this.userService.UpdateAsync(userModel);

        // Assert
        this.userRepositoryMock.Verify(r => r.UpdateAsync(existingUser), Times.Once);
        Assert.Equal(userModel.FirstName, existingUser.FirstName);
        Assert.Equal(userModel.LastName, existingUser.LastName);
    }
}
