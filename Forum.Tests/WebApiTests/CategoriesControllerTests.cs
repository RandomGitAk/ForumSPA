using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApp.BusinessLogic.Interfaces;
using WebApp.BusinessLogic.Models.CategoryModels;
using WebApp.BusinessLogic.Models;
using WebApp.WebApi.Controllers;
using WebApp.WebApi.ViewModels;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Forum.Tests.WebApiTests;
public class CategoriesControllerTests
{
    private readonly Mock<ICategoryService> categoryServiceMock;
    private readonly CategoriesController categoriesController;
    private readonly Mock<ILogger<CategoriesController>> loggerMock;

    public CategoriesControllerTests()
    {
        this.categoryServiceMock = new Mock<ICategoryService>();
        this.loggerMock = new Mock<ILogger<CategoriesController>>();
        this.categoriesController = new CategoriesController(this.categoryServiceMock.Object, this.loggerMock.Object);
    }

    [Fact]
    public async Task GetShouldReturnOkResultWithCategories()
    {
        // Arrange
        var categories = new List<CategoryModel>
        {
            new CategoryModel { Id = 1, Name = "Category 1" },
            new CategoryModel { Id = 2, Name = "Category 2" }
        };
        _ = this.categoryServiceMock.Setup(service => service.GetAllAsync()).ReturnsAsync(categories);

        // Act
        var result = await this.categoriesController.Get();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCategories = Assert.IsType<List<CategoryModel>>(okResult.Value);
        Assert.Equal(2, returnedCategories.Count);
    }

    [Fact]
    public void GetPagedShouldReturnOkResultWithPaginatedCategories()
    {
        // Arrange
        var paginationParams = new PaginationParamsFromQueryModel();
        var paginatedCategories = new CategoryModelPagination
        {
            TotalPages = 10,
            Items =
            [
                new CategoryModel { Id = 1, Name = "Category 1" }
            ]
        };
        _ = this.categoryServiceMock.Setup(service => service.GetCategoriesPaged(paginationParams)).Returns(paginatedCategories);

        // Act
        var result = this.categoriesController.GetPaged(paginationParams);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedPagination = Assert.IsType<CategoryModelPagination>(okResult.Value);
        Assert.Equal(10, returnedPagination.TotalPages);
        _ = Assert.Single(returnedPagination!.Items!);
    }

    [Fact]
    public async Task PostShouldReturnCreatedResultWithCategory()
    {
        // Arrange
        var categoryInputModel = new CategoryInputAddModel { Name = "New Category", Description = "Description" };
        var categoryModel = new CategoryModel { Id = 1, Name = "New Category", Description = "Description" };
        _ = this.categoryServiceMock.Setup(service => service.AddAsync(It.IsAny<CategoryModel>())).ReturnsAsync(categoryModel);

        // Act
        var result = await this.categoriesController.Post(categoryInputModel);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(this.categoriesController.Get), createdResult.ActionName);
        Assert.Equal(categoryModel.Id, createdResult!.RouteValues!["id"]);
    }

    [Fact]
    public async Task PutShouldReturnNoContentWhenCategoryExists()
    {
        // Arrange
        var categoryInputModel = new CategoryInputAddModel { Name = "Updated Category", Description = "Updated Description" };
        var categoryModel = new CategoryModel { Id = 1, Name = "Updated Category", Description = "Updated Description" };
        _ = this.categoryServiceMock.Setup(service => service.UpdateAsync(categoryModel)).Returns(Task.CompletedTask);

        // Act
        var result = await this.categoriesController.Put(1, categoryInputModel);

        // Assert
        _ = Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task PutShouldReturnNotFoundWhenCategoryDoesNotExist()
    {
        // Arrange
        var categoryInputModel = new CategoryInputAddModel { Name = "Non-existent Category", Description = "Non-existent Description" };
        _ = this.categoryServiceMock.Setup(service => service.UpdateAsync(It.IsAny<CategoryModel>())).Throws(new InvalidOperationException());

        // Act
        var result = await this.categoriesController.Put(999, categoryInputModel);

        // Assert
        _ = Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteShouldReturnNoContentWhenCategoryExists()
    {
        // Arrange
        _ = this.categoryServiceMock.Setup(service => service.DeleteAsync(1)).Returns(Task.CompletedTask);

        // Act
        var result = await this.categoriesController.Delete(1);

        // Assert
        _ = Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteShouldReturnNotFoundWhenCategoryDoesNotExist()
    {
        // Arrange
        _ = this.categoryServiceMock.Setup(service => service.DeleteAsync(It.IsAny<int>())).Throws(new InvalidOperationException());

        // Act
        var result = await this.categoriesController.Delete(999);

        // Assert
        _ = Assert.IsType<NotFoundResult>(result);
    }
}
