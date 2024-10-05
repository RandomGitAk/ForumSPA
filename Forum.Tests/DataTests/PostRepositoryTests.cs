using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Data;
using WebApp.DataAccess.Entities.pages;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Repositories;
using Xunit;

namespace Forum.Tests.DataTests;
public class PostRepositoryTests
{
    private readonly ApplicationContext context;
    private readonly PostRepository postRepository;

    public PostRepositoryTests()
    {
        // Set up an in-memory database
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        this.context = new ApplicationContext(options);
        this.postRepository = new PostRepository(this.context);
    }

    [Fact]
    public void GetAllPostsWithDetailsShouldReturnPosts()
    {
        // Arrange
        var users = new List<User>
        {
            new User
            {
                Id = 1,
                FirstName = "test",
                Email = "test@example.com",
                HashedPasssword = "hashed_password",
                Image = "image_url",
                LastName = "Doe",
                Salt = "salt_value"
            },
            new User
            {
                Id = 2,
                FirstName = "test",
                Email = "test@example.com",
                HashedPasssword = "hashed_password",
                Image = "image_url",
                LastName = "Doe",
                Salt = "salt_value"
            },
            new User
            {
                Id = 3, FirstName = "test",
                Email = "test@example.com",
                HashedPasssword = "hashed_password",
                Image = "image_url",
                LastName = "Doe",
                Salt = "salt_value"
            }
        };

        var categories = new List<Category>
        {
            new Category { Id = 1, Name = "Category 1", Description = "desc" },
            new Category { Id = 2, Name = "Category 2", Description = "desc" },
            new Category { Id = 3, Name = "Category 3", Description = "desc" },
        };

        var posts = new List<Post>
        {
            new Post { Id = 1, Content = "content", Title = "Post 1", UserId = 1, User = users[0], CategoryId = 1,
            Category = categories[0], PostedDate = DateTime.Now },
            new Post { Id = 2, Content = "content", Title = "Post 2", UserId = 2,  User = users[1], CategoryId = 2,
            Category = categories[1],  PostedDate = DateTime.Now.AddDays(-1) },
            new Post { Id = 3, Content = "content", Title = "Post 3", UserId = 3, User = users[2],  CategoryId = 3,
            Category = categories[2],  PostedDate = DateTime.Now.AddDays(-1) }
        };
        this.context.Users.AddRange(users);
        this.context.Categories.AddRange(categories);
        this.context.Posts.AddRange(posts);
        _ = this.context.SaveChanges();

        var options = new QueryOptions { CurrentPage = 1, PageSize = 2 };

        // Act
        var result = this.postRepository.GetAllPostsWithDetails(options);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(3, result.TotalItems);
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public void GetPostsByUserIdAsyncShouldReturnUserPosts()
    {
        // Arrange
        var users = new List<User>
        {
            new User
            {
                Id = 4,
                FirstName = "test",
                Email = "test@example.com",
                HashedPasssword = "hashed_password",
                Image = "image_url",
                LastName = "Doe",
                Salt = "salt_value"
            },
            new User
            {
                Id = 5,
                FirstName = "test",
                Email = "test@example.com",
                HashedPasssword = "hashed_password",
                Image = "image_url",
                LastName = "Doe",
                Salt = "salt_value"
            },
        };

        var categories = new List<Category>
        {
            new Category { Id = 4, Name = "Category 1", Description = "desc" },
            new Category { Id = 5, Name = "Category 2", Description = "desc" },
            new Category { Id = 6, Name = "Category 3", Description = "desc" },
        };

        var posts = new List<Post>
        {
            new Post { Id = 4, Content = "content", Title = "Post 1", UserId = 4, User = users[0], CategoryId = 1,
            Category = categories[0], PostedDate = DateTime.Now },
            new Post { Id = 5, Content = "content", Title = "Post 2", UserId = 4,  User = users[0], CategoryId = 2,
            Category = categories[1],  PostedDate = DateTime.Now.AddDays(-1) },
            new Post { Id = 6, Content = "content", Title = "Post 3", UserId = 5, User = users[1],  CategoryId = 3,
            Category = categories[2],  PostedDate = DateTime.Now.AddDays(-1) }
        };
        this.context.Users.AddRange(users);
        this.context.Categories.AddRange(categories);
        this.context.Posts.AddRange(posts);
        _ = this.context.SaveChanges();

        var options = new QueryOptions { CurrentPage = 1, PageSize = 2 };

        // Act
        var result = this.postRepository.GetPostsByUserIdAsync(options, 4);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetPostWithDetailsByIdAsyncShouldReturnPostDetails()
    {
        // Arrange
        var user = new User
        {
            Id = 4,
            FirstName = "User 1",
            Email = "test@example.com",
            HashedPasssword = "hashed_password",
            Image = "image_url",
            LastName = "Doe",
            Salt = "salt_value"
        };
        var category = new Category { Id = 4, Name = "Category 1", Description = "desc" };
        var likes = new List<PostLike> { new PostLike { PostId = 7, UserId = 4, IsLike = true } };

        var post = new Post { Id = 7, Content = "content", Title = "Post 1", UserId = 4, User = user, CategoryId = 4, Category = category, Likes = likes, PostedDate = DateTime.Now };
        _ = this.context.Users.AddAsync(user);
        _ = this.context.Categories.AddAsync(category);
        _ = this.context.PostLikes.AddRangeAsync(likes);
        _ = await this.context.Posts.AddAsync(post);
        _ = await this.context.SaveChangesAsync();

        // Act
        var result = await this.postRepository.GetPostWithDetailsByIdAsync(post.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Post 1", result!.Title);
        Assert.Equal("User 1", result.User!.FirstName);
        Assert.Equal("Category 1", result!.Category!.Name);
        _ = Assert.Single(result!.Likes!);
    }

    [Fact]
    public async Task GetPostWithDetailsByIdAsyncShouldReturnNullWhenNotFound()
    {
        // Act
        var result = await this.postRepository.GetPostWithDetailsByIdAsync(99);

        // Assert
        Assert.Null(result);
    }
}
