using AutoMapper;
using Moq;
using WebApp.BusinessLogic.Models;
using WebApp.BusinessLogic.Models.CategoryModels;
using WebApp.BusinessLogic.Services;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Entities.pages;
using WebApp.DataAccess.Interfaces;
using Xunit;

namespace Forum.Tests.BussinesTests;
public class CategoryServiceTests
{
    private readonly Mock<ICategory> categoryRepositoryMock;
    private readonly Mock<IMapper> mapperMock;
    private readonly CategoryService categoryService;

    public CategoryServiceTests()
    {
        this.categoryRepositoryMock = new Mock<ICategory>();
        this.mapperMock = new Mock<IMapper>();
        this.categoryService = new CategoryService(this.categoryRepositoryMock.Object, this.mapperMock.Object);
    }

    [Fact]
    public async Task AddAsyncShouldAddCategory()
    {
        // Arrange
        var categoryModel = new CategoryModel { Id = 1, Name = "New Category" };
        var category = new Category { Id = 1, Name = "New Category" };

        _ = this.mapperMock.Setup(m => m.Map<Category>(It.IsAny<CategoryModel>())).Returns(category);
        _ = this.categoryRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);
        _ = this.mapperMock.Setup(m => m.Map<CategoryModel>(It.IsAny<Category>())).Returns(categoryModel);

        // Act
        var result = await this.categoryService.AddAsync(categoryModel);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(categoryModel.Id, result.Id);
        this.categoryRepositoryMock.Verify(r => r.AddAsync(category), Times.Once);
    }

    [Fact]
    public async Task DeleteAsyncShouldThrowExceptionWhenCategoryNotFound()
    {
        // Arrange
        int categoryId = 1;
        _ = this.categoryRepositoryMock.Setup(r => r.GetByIdAsync(categoryId)).ReturnsAsync((Category)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => this.categoryService.DeleteAsync(categoryId));
        Assert.Equal("Category not found.", exception.Message);
    }

    [Fact]
    public async Task DeleteAsyncShouldDeleteCategoryWhenCategoryExists()
    {
        // Arrange
        int categoryId = 1;
        var existingCategory = new Category { Id = categoryId, Name = "Existing Category" };
        _ = this.categoryRepositoryMock.Setup(r => r.GetByIdAsync(categoryId)).ReturnsAsync(existingCategory);
        _ = this.categoryRepositoryMock.Setup(r => r.DeleteByIdAsync(categoryId)).Returns(Task.CompletedTask);

        // Act
        await this.categoryService.DeleteAsync(categoryId);

        // Assert
        this.categoryRepositoryMock.Verify(r => r.DeleteByIdAsync(categoryId), Times.Once);
    }

    [Fact]
    public async Task GetAllAsyncShouldReturnAllCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new Category { Id = 1, Name = "Category 1" },
            new Category { Id = 2, Name = "Category 2" }
        };
        var categoryModels = new List<CategoryModel>
        {
            new CategoryModel { Id = 1, Name = "Category 1" },
            new CategoryModel { Id = 2, Name = "Category 2" }
        };

        _ = this.categoryRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);
        _ = this.mapperMock.Setup(m => m.Map<IEnumerable<CategoryModel>>(It.IsAny<IEnumerable<Category>>())).Returns(categoryModels);

        // Act
        var result = await this.categoryService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsyncShouldReturnCategoryWhenCategoryExists()
    {
        // Arrange
        int categoryId = 1;
        var existingCategory = new Category { Id = categoryId, Name = "Existing Category" };
        var categoryModel = new CategoryModel { Id = categoryId, Name = "Existing Category" };

        _ = this.categoryRepositoryMock.Setup(r => r.GetByIdAsync(categoryId)).ReturnsAsync(existingCategory);
        _ = this.mapperMock.Setup(m => m.Map<CategoryModel>(It.IsAny<Category>())).Returns(categoryModel);

        // Act
        var result = await this.categoryService.GetByIdAsync(categoryId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingCategory.Id, result.Id);
    }

    [Fact]
    public void GetCategoriesPagedShouldReturnPagedCategories()
    {
        // Arrange
        var paginationParams = new PaginationParamsFromQueryModel { Page = 0, PerPage = 10 };

        var categories = new List<Category>
        {
            new Category { Id = 1, Name = "Category 1" },
            new Category { Id = 2, Name = "Category 2" }
        }.AsQueryable();

        var queryOptions = new QueryOptions
        {
            CurrentPage = paginationParams.Page + 1,
            PageSize = paginationParams.PerPage,
            SearchPropertyName = null!,
            SearchTerm = null
        };

        var pagedList = new PagedList<Category>(categories, queryOptions);

        _ = this.categoryRepositoryMock
            .Setup(r => r.GetCategoriesPaged(It.Is<QueryOptions>(q =>
                q.CurrentPage == queryOptions.CurrentPage &&
                q.PageSize == queryOptions.PageSize &&
                q.SearchPropertyName == queryOptions.SearchPropertyName &&
                q.SearchTerm == queryOptions.SearchTerm)))
            .Returns(pagedList);

        _ = this.mapperMock.Setup(m => m.Map<CategoryModelPagination>(It.IsAny<PagedList<Category>>()))
                           .Returns(new CategoryModelPagination
                           {
                               Total = pagedList.TotalItems,
                               Page = pagedList.CurrentPage,
                               PerPage = pagedList.PageSize,
                               TotalPages = pagedList.TotalPages,
                               Items = this.mapperMock.Object.Map<IEnumerable<CategoryModel>>(pagedList)
                           });

        // Act
        var result = this.categoryService.GetCategoriesPaged(paginationParams);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Total);
    }




    [Fact]
    public async Task UpdateAsyncShouldThrowExceptionWhenCategoryNotFound()
    {
        // Arrange
        var categoryModel = new CategoryModel { Id = 1, Name = "Updated Category" };
        _ = this.categoryRepositoryMock.Setup(r => r.GetByIdAsync(categoryModel.Id)).ReturnsAsync((Category)null!);

        // Act & Assert
        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => this.categoryService.UpdateAsync(categoryModel));
    }

    [Fact]
    public async Task UpdateAsyncShouldUpdateCategoryWhenCategoryExists()
    {
        // Arrange
        var categoryModel = new CategoryModel { Id = 1, Name = "Updated Category" };
        var existingCategory = new Category { Id = 1, Name = "Existing Category" };

        _ = this.categoryRepositoryMock.Setup(r => r.GetByIdAsync(categoryModel.Id)).ReturnsAsync(existingCategory);
        _ = this.mapperMock.Setup(m => m.Map<Category>(It.IsAny<CategoryModel>())).Returns(existingCategory);
        _ = this.categoryRepositoryMock.Setup(r => r.UpdateAsync(existingCategory)).Returns(Task.CompletedTask);

        // Act
        await this.categoryService.UpdateAsync(categoryModel);

        // Assert
        this.categoryRepositoryMock.Verify(r => r.UpdateAsync(existingCategory), Times.Once);
    }
}
