using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Data;
using WebApp.DataAccess.Entities.pages;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Repositories;
using Xunit;

namespace Forum.Tests.DataTests
{

    public class CategoryRepositoryTests
    {
        private readonly ApplicationContext context;
        private readonly CategoryRepository categoryRepository;

        public CategoryRepositoryTests()
        {
            // Set up an in-memory database
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                  .EnableSensitiveDataLogging()
              .Options;


            this.context = new ApplicationContext(options);

            this.categoryRepository = new CategoryRepository(this.context);

        }

        [Fact]
        public async Task GetCategoriesPagedReturnsPagedListOfCategories()
        {
            // Arrange
            var existingCategories = await this.context.Categories.ToListAsync();
            var categories = new List<Category>
        {
            new Category {Id = 1, Name = "Category 1", Description = "Description 1" },
            new Category {Id = 2, Name = "Category 2", Description = "Description 2" },
            new Category {Id = 3, Name = "Category 3", Description = "Description 3" },
        };

            await this.context.Categories.AddRangeAsync(categories);
            _ = await this.context.SaveChangesAsync();

            var options = new QueryOptions
            {
                CurrentPage = 1,
                PageSize = 2
            };

            // Act
            var result = this.categoryRepository.GetCategoriesPaged(options);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(3 + existingCategories.Count, result.TotalItems);
            Assert.Equal(existingCategories.Count > 0 ? 3 : 2, result.TotalPages);
        }

        [Fact]
        public async Task AddAsyncShouldAddCategory()
        {
            // Arrange
            var category = new Category { Id = 4, Name = "Test Category", Description = "Test Description" };

            // Act
            await this.categoryRepository.AddAsync(category);

            // Assert
            var result = await this.context.Categories.FindAsync(category.Id);
            Assert.NotNull(result);
            Assert.Equal("Test Category", result.Name);
        }

        [Fact]
        public void DeleteShouldRemoveCategory()
        {
            // Arrange
            var category = new Category { Id = 5, Name = "Test Category", Description = "Test Description" };
            _ = this.context.Categories.Add(category);
            _ = this.context.SaveChanges();

            // Act
            this.categoryRepository.Delete(category);

            // Assert
            var result = this.context.Categories.Find(5);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteByIdAsyncShouldRemoveCategoryById()
        {
            // Arrange
            var category = new Category { Id = 6, Name = "Test Category", Description = "Test Description" };
            _ = await this.context.Categories.AddAsync(category);
            _ = await this.context.SaveChangesAsync();

            // Act
            await this.categoryRepository.DeleteByIdAsync(category.Id);

            // Assert
            var result = await this.context.Categories.FindAsync(6);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsyncShouldReturnAllCategories()
        {
            // Arrange
            var existingCategories = await this.context.Categories.ToListAsync();
            _ = await this.context.Categories.AddAsync(new Category { Id = 7, Name = "Category 1", Description = "Desc 1" });
            _ = await this.context.Categories.AddAsync(new Category { Id = 8, Name = "Category 2", Description = "Desc 2" });
            _ = await this.context.SaveChangesAsync();

            // Act
            var categories = await this.categoryRepository.GetAllAsync();

            // Assert
            Assert.Equal(2 + existingCategories.Count, categories.Count());
        }

        [Fact]
        public async Task GetByIdAsyncShouldReturnCategoryById()
        {
            // Arrange
            var category = new Category { Id = 9, Name = "Test Category", Description = "Test Description" };
            _ = await this.context.Categories.AddAsync(category);
            _ = await this.context.SaveChangesAsync();

            // Act
            var result = await this.categoryRepository.GetByIdAsync(category.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Category", result.Name);
        }

        [Fact]
        public async Task GetByIdAsyncShouldReturnNullForInvalidId()
        {
            // Act
            var result = await this.categoryRepository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsyncShouldUpdateCategory()
        {
            // Arrange
            var category = new Category { Id = 10, Name = "Old Category", Description = "Old Description" };
            _ = await this.context.Categories.AddAsync(category);
            _ = await this.context.SaveChangesAsync();

            // Act
            category.Name = "Updated Category";
            await this.categoryRepository.UpdateAsync(category);

            // Assert
            var updatedCategory = await this.context.Categories.FindAsync(category.Id);
            Assert.NotNull(updatedCategory);
            Assert.Equal("Updated Category", updatedCategory.Name);
        }

        [Fact]
        public async Task UpdateAsyncShouldNotUpdateNonExistingCategory()
        {
            // Arrange
            var category = new Category { Id = 999, Name = "Non-existing", Description = "No Description" };

            // Act
            await this.categoryRepository.UpdateAsync(category);

            // Assert
            var result = await this.context.Categories.FindAsync(999);
            Assert.Null(result);
        }
    }
}

