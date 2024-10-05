using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Data;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Repositories;
using Xunit;

namespace Forum.Tests.DataTests;
public class RoleRepositoryTests
{
    private readonly ApplicationContext context;
    private readonly RoleRepository roleRepository;

    public RoleRepositoryTests()
    {
        // Set up an in-memory database
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        this.context = new ApplicationContext(options);
        this.roleRepository = new RoleRepository(this.context);
    }

    [Fact]
    public async Task FindByNameAsyncShouldReturnCorrectRoleWhenRoleExists()
    {
        // Arrange
        var roles = new List<Role>
        {
            new Role { Id = 1, Name = "Admin" },
            new Role { Id = 2, Name = "User" },
            new Role { Id = 3, Name = "Moderator" }
        };
        await this.context.Roles.AddRangeAsync(roles);
        _ = await this.context.SaveChangesAsync();

        // Act
        var result = await this.roleRepository.FindByNameAsync("Admin");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Admin", result!.Name);
        Assert.Equal(1, result!.Id);
    }

    [Fact]
    public async Task FindByNameAsyncShouldReturnNullWhenRoleDoesNotExist()
    {
        // Arrange
        var roles = new List<Role>
        {
            new Role { Id = 1, Name = "Admin" },
            new Role { Id = 2, Name = "User" },
            new Role { Id = 3, Name = "Moderator" }
        };
        await this.context.Roles.AddRangeAsync(roles);
        _ = await this.context.SaveChangesAsync();

        // Act
        var result = await this.roleRepository.FindByNameAsync("NonExistentRole");

        // Assert
        Assert.Null(result);
    }
}
