using AutoMapper;
using Moq;
using WebApp.BusinessLogic.Models.RoleModels;
using WebApp.BusinessLogic.Services;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Interfaces;
using Xunit;

namespace Forum.Tests.BussinesTests;
public class RoleServiceTests
{
    private readonly Mock<IRole> roleRepositoryMock;
    private readonly Mock<IMapper> mapperMock;
    private readonly RoleService roleService;

    public RoleServiceTests()
    {
        this.roleRepositoryMock = new Mock<IRole>();
        this.mapperMock = new Mock<IMapper>();
        this.roleService = new RoleService(this.roleRepositoryMock.Object, this.mapperMock.Object);
    }

    [Fact]
    public async Task GetAllAsyncShouldReturnAllRoles()
    {
        // Arrange
        var roles = new List<Role>
        {
            new Role { Id = 1, Name = "Admin" },
            new Role { Id = 2, Name = "User" }
        };
        var roleModels = new List<RoleModel>
        {
            new RoleModel { Id = 1, Name = "Admin" },
            new RoleModel { Id = 2, Name = "User" }
        };

        _ = this.roleRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(roles);
        _ = this.mapperMock.Setup(m => m.Map<IEnumerable<RoleModel>>(It.IsAny<IEnumerable<Role>>())).Returns(roleModels);

        // Act
        var result = await this.roleService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsyncShouldReturnRoleWhenRoleExists()
    {
        // Arrange
        int roleId = 1;
        var existingRole = new Role { Id = roleId, Name = "Admin" };
        var roleModel = new RoleModel { Id = roleId, Name = "Admin" };

        _ = this.roleRepositoryMock.Setup(r => r.GetByIdAsync(roleId)).ReturnsAsync(existingRole);
        _ = this.mapperMock.Setup(m => m.Map<RoleModel>(It.IsAny<Role>())).Returns(roleModel);

        // Act
        var result = await this.roleService.GetByIdAsync(roleId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingRole.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsyncShouldReturnNullWhenRoleNotFound()
    {
        // Arrange
        int roleId = 1;
        _ = this.roleRepositoryMock.Setup(r => r.GetByIdAsync(roleId)).ReturnsAsync((Role)null!);

        // Act
        var result = await this.roleService.GetByIdAsync(roleId);

        // Assert
        Assert.Null(result);
    }
}
