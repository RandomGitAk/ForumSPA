using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Data;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Repositories;
using Xunit;

namespace Forum.Tests.DataTests;
public class CommentRepositoryTests
{
    private readonly ApplicationContext context;
    private readonly CommentRepository commentRepository;

    public CommentRepositoryTests()
    {
        // Set up an in-memory database
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
              .EnableSensitiveDataLogging()
              .Options;


        this.context = new ApplicationContext(options);
        this.commentRepository = new CommentRepository(this.context);

    }

    [Fact]
    public async Task GetByIdAsyncShouldReturnCommentWhenExists()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            FirstName = "test",
            Email = "test@example.com",
            HashedPasssword = "hashed_password",
            Image = "image_url",
            LastName = "Doe",
            Salt = "salt_value"
        };
        _ = await this.context.Users.AddAsync(user);
        _ = await this.context.SaveChangesAsync();

        var comment = new Comment
        {
            Id = 1,
            PostId = 1,
            UserId = user.Id,
            Content = "Test Comment",
            CommentDate = DateTime.Now
        };
        _ = await this.context.Comments.AddAsync(comment);
        _ = await this.context.SaveChangesAsync();

        // Act
        var result = await this.commentRepository.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(comment.Content, result.Content);
        Assert.Equal(comment.UserId, result.UserId);
    }

    [Fact]
    public async Task GetByIdAsyncShouldReturnNullWhenNotExists()
    {
        // Act
        var result = await this.commentRepository.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByPostIdAsyncShouldReturnCommentsWhenPostExists()
    {
        // Arrange
        var postId = 1;

        var user1 = new User
        {
            Id = 2,
            FirstName = "test1",
            Email = "user1@example.com",
            HashedPasssword = "hashed_password1",
            Image = "image_url1",
            LastName = "Doe",
            Salt = "salt1",
        };

        var user2 = new User
        {
            Id = 3,
            FirstName = "test2",
            Email = "user2@example.com",
            HashedPasssword = "hashed_password2",
            Image = "image_url2",
            LastName = "Smith",
            Salt = "salt2",
        };

        await this.context.Users.AddRangeAsync(user1, user2);
        _ = await this.context.SaveChangesAsync();

        var comments = new List<Comment>
        {
            new Comment { Id = 2, PostId = postId, UserId = user1.Id, Content = "Comment 1", CommentDate = DateTime.Now },
            new Comment { Id = 3, PostId = postId, UserId = user2.Id, Content = "Comment 2", CommentDate = DateTime.Now.AddMinutes(-1) },
            new Comment { Id = 4, PostId = postId, UserId = user1.Id, Content = "Comment 3", CommentDate = DateTime.Now.AddMinutes(-2), ParentCommentId = 1 }
        };

        await this.context.Comments.AddRangeAsync(comments);
        _ = await this.context.SaveChangesAsync();

        // Act
        var result = await this.commentRepository.GetByPostIdAsync(postId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, c => Assert.Equal(postId, c.PostId));
        Assert.All(result, c => Assert.Null(c.ParentCommentId));
    }

    [Fact]
    public async Task GetByPostIdAsyncShouldReturnEmptyWhenNoCommentsExist()
    {
        // Act
        var result = await this.commentRepository.GetByPostIdAsync(999);

        // Assert
        Assert.Empty(result);
    }
}
