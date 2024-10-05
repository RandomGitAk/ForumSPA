using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Data;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Repositories;
using Xunit;

namespace Forum.Tests.DataTests;
public class UserRepositoryTests
{
    private readonly ApplicationContext context;
    private readonly UserRepository userRepository;

    public UserRepositoryTests()
    {
        // Set up an in-memory database
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        this.context = new ApplicationContext(options);
        this.userRepository = new UserRepository(this.context);
    }

    [Fact]
    public async Task GetUserByEmailAsyncShouldReturnUserWhenUserExists()
    {
        // Arrange
        var role = new Role { Id = 1, Name = "Admin" };
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Image = "...",
            HashedPasssword = "hashed_password",
            Salt = "salt_value",
            Role = role
        };
        _ = await this.context.Roles.AddAsync(role);
        _ = this.context.Users.AddAsync(user);
        _ = await this.context.SaveChangesAsync();

        // Act
        var result = await this.userRepository.GetUserByEmailAsync("test@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result!.Email);
        Assert.Equal("Admin", result!.Role?.Name);
    }

    [Fact]
    public async Task GetUserByEmailAsyncShouldReturnNullWhenUserDoesNotExist()
    {
        // Act
        var result = await this.userRepository.GetUserByEmailAsync("nonexistent@example.com");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserSaltAsyncShouldReturnSaltWhenUserExists()
    {
        // Arrange
        var user = new User
        {
            Id = 2,
            FirstName = "User",
            LastName = "last",
            Image = "...",
            Email = "test@example.com",
            HashedPasssword = "hashed_password",
            Salt = "salt_value"
        };
        _ = await this.context.Users.AddAsync(user);
        _ = await this.context.SaveChangesAsync();

        // Act
        var result = await this.userRepository.GetUserSaltAsync("test@example.com");

        // Assert
        Assert.Equal("salt_value", result);
    }

    [Fact]
    public async Task GetUserSaltAsyncShouldReturnNullWhenUserDoesNotExist()
    {
        // Act
        var result = await this.userRepository.GetUserSaltAsync("nonexistent@example.com");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task IsUserExistsAsyncShouldReturnTrueWhenUserExists()
    {
        // Arrange
        var user = new User
        {
            Id = 3,
            FirstName = "User",
            LastName = "last",
            Image = "...",
            Email = "test@example.com",
            HashedPasssword = "hashed_password",
            Salt = "salt_value"
        };
        _ = await this.context.Users.AddAsync(user);
        _ = await this.context.SaveChangesAsync();

        // Act
        var result = await this.userRepository.IsUserExistsAsync("test@example.com");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsUserExistsAsyncShouldReturnFalseWhenUserDoesNotExist()
    {
        // Act
        var result = await this.userRepository.IsUserExistsAsync("nonexistent@example.com");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task VerifyUserAsyncShouldReturnTrueWhenCredentialsAreCorrect()
    {
        // Arrange
        var user = new User
        {
            Id = 4,
            FirstName = "User",
            LastName = "last",
            Image = "...",
            Email = "test@example.com",
            HashedPasssword = "hashed_password",
            Salt = "salt_value"
        };
        _ = this.context.Users.AddAsync(user);
        _ = await this.context.SaveChangesAsync();

        // Act
        var result = await this.userRepository.VerifyUserAsync(user);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task VerifyUserAsyncShouldReturnFalseWhenCredentialsAreIncorrect()
    {
        // Arrange
        var user = new User
        {
            Id = 5,
            FirstName = "User",
            LastName = "last",
            Image = "...",
            Email = "test@example.com",
            HashedPasssword = "wrong_password",
            Salt = "salt_value"
        };
        _ = this.context.Users.AddAsync(user);
        _ = await this.context.SaveChangesAsync();

        var userToVerify = new User
        {
            Email = "test@example.com",
            HashedPasssword = "different_password"
        };

        // Act
        var result = await this.userRepository.VerifyUserAsync(userToVerify);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task StoreRefreshTokenAsyncShouldStoreRefreshTokenForUser()
    {
        // Arrange
        var role = new Role
        {
            Id = 1,
            Name = "UserRole"
        };
        var user = new User
        {
            Id = 6,
            FirstName = "User",
            LastName = "last",
            Image = "...",
            Email = "test@example.com",
            HashedPasssword = "hashed_password",
            Salt = "salt_value",
            Role = role
        };
        _ = this.context.Users.AddAsync(user);
        _ = await this.context.SaveChangesAsync();

        var refreshToken = "new_refresh_token";
        var expiryDate = DateTime.UtcNow.AddDays(7);

        // Act
        await this.userRepository.StoreRefreshTokenAsync(user.Id, refreshToken, expiryDate);

        // Assert
        var updatedUser = await this.context.Users.FindAsync(user.Id);
        Assert.Equal(refreshToken, updatedUser?.RefreshToken);
        Assert.Equal(expiryDate, updatedUser?.RefreshTokenExpiryDate);
    }

    [Fact]
    public async Task DeleteRefreshTokenAsyncShouldRemoveRefreshTokenForUser()
    {
        // Arrange
        var role = new Role
        {
            Id = 2,
            Name = "UserRole"
        };
        var user = new User
        {
            Id = 7,
            FirstName = "User",
            LastName = "Last",
            Image = "...",
            Email = "test@example.com",
            HashedPasssword = "hashed_password",
            Salt = "salt_value",
            RefreshToken = "existing_refresh_token",
            RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(7),
            Role = role
        };

        _ = await this.context.Roles.AddAsync(role);
        _ = await this.context.Users.AddAsync(user);
        _ = await this.context.SaveChangesAsync();

        // Act
        await this.userRepository.DeleteRefreshTokenAsync(user.Id);

        // Assert
        var updatedUser = await this.context.Users.FindAsync(user.Id);
        Assert.NotNull(updatedUser);
        Assert.Null(updatedUser!.RefreshToken);
        Assert.Null(updatedUser!.RefreshTokenExpiryDate);
    }

    [Fact]
    public async Task IsRefreshTokenValidAsyncShouldReturnTrueWhenRefreshTokenIsValid()
    {
        // Arrange
        var role = new Role
        {
            Id = 3,
            Name = "UserRole"
        };
        var user = new User
        {
            Id = 8,
            FirstName = "User",
            LastName = "last",
            Image = "...",
            Email = "test@example.com",
            HashedPasssword = "hashed_password",
            Salt = "salt_value",
            RefreshToken = "valid_refresh_token",
            RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(7),
            Role = role,
        };
        _ = this.context.Users.AddAsync(user);
        _ = await this.context.SaveChangesAsync();

        // Act
        var result = await this.userRepository.IsRefreshTokenValidAsync(user.Id, "valid_refresh_token");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsRefreshTokenValidAsyncShouldReturnFalseWhenRefreshTokenIsExpired()
    {
        // Arrange
        var user = new User
        {
            Id = 9,
            FirstName = "User",
            LastName = "last",
            Image = "...",
            Email = "test@example.com",
            HashedPasssword = "hashed_password",
            Salt = "salt_value",
            RefreshToken = "expired_refresh_token",
            RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(-1)
        };
        _ = await this.context.Users.AddAsync(user);
        _ = await this.context.SaveChangesAsync();

        // Act
        var result = await this.userRepository.IsRefreshTokenValidAsync(user.Id, "expired_refresh_token");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetUserByRefreshTokenAsyncShouldReturnUserWhenRefreshTokenIsValid()
    {
        // Arrange
        var user = new User
        {
            Id = 10,
            FirstName = "User",
            LastName = "last",
            Image = "...",
            Email = "test@example.com",
            HashedPasssword = "hashed_password",
            Salt = "salt_value",
            RefreshToken = "valid_refresh_token"
        };
        _ = await this.context.Users.AddAsync(user);
        _ = await this.context.SaveChangesAsync();

        // Act
        var result = await this.userRepository.GetUserByRefreshTokenAsync("valid_refresh_token");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result!.Email);
    }
}
