using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WebApp.BusinessLogic.Interfaces;
using WebApp.BusinessLogic.Models.RoleModels;
using WebApp.WebApi.Controllers;
using Xunit;

namespace Forum.Tests.WebApiTests;
public class RolesControllerTests
{
    private readonly Mock<IRoleService> roleServiceMock;
    private readonly RolesController rolesController;
    private readonly Mock<ILogger<RolesController>> loggerMock;

    public RolesControllerTests()
    {
        this.roleServiceMock = new Mock<IRoleService>();
        this.loggerMock = new Mock<ILogger<RolesController>>();
        this.rolesController = new RolesController(this.roleServiceMock.Object, this.loggerMock.Object);
    }

    [Fact]
    public async Task GetShouldReturnOkResultWithRoles()
    {
        // Arrange
        var roles = new List<RoleModel>
        {
            new RoleModel { Id = 1, Name = "Admin" },
            new RoleModel { Id = 2, Name = "User" }
        };

        _ = this.roleServiceMock.Setup(service => service.GetAllAsync())
                                .ReturnsAsync(roles);

        // Act
        var result = await this.rolesController.Get();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedRoles = Assert.IsAssignableFrom<IEnumerable<RoleModel>>(okResult.Value);
        Assert.Equal(roles, returnedRoles);
    }

    [Fact]
    public async Task GetShouldReturnEmptyListWhenNoRolesExist()
    {
        // Arrange
        _ = this.roleServiceMock.Setup(service => service.GetAllAsync())
                                .ReturnsAsync([]);

        // Act
        var result = await this.rolesController.Get();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedRoles = Assert.IsAssignableFrom<IEnumerable<RoleModel>>(okResult.Value);
        Assert.Empty(returnedRoles);
    }
}
