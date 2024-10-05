using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Data;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Repositories;
using Xunit;

namespace Forum.Tests.DataTests;
public class CommentLikeRepositoryTests
{
    private readonly ApplicationContext context;
    private readonly CommentLikeRepository commentLikeRepository;

    public CommentLikeRepositoryTests()
    {
        // Set up an in-memory database
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
              .EnableSensitiveDataLogging()
              .Options;

        this.context = new ApplicationContext(options);
        this.commentLikeRepository = new CommentLikeRepository(this.context);

    }

    [Fact]
    public async Task AddAsyncShouldAddCommentLike()
    {
        // Arrange
        var commentLike = new CommentLike { CommentId = 1, UserId = 1 };

        // Act
        await this.commentLikeRepository.AddAsync(commentLike);

        // Assert
        var result = await this.context.CommentLikes.FirstOrDefaultAsync(e => e.UserId == commentLike.UserId && e.CommentId == commentLike.CommentId);
        Assert.Equal(1, result!.CommentId);
        Assert.Equal(1, result.UserId);
    }

    [Fact]
    public void DeleteShouldRemoveCommentLike()
    {
        // Arrange
        var commentLike = new CommentLike { CommentId = 2, UserId = 2 };
        _ = this.context.CommentLikes.Add(commentLike);
        _ = this.context.SaveChanges();

        // Act
        this.commentLikeRepository.Delete(commentLike);

        // Assert
        var result = this.context.CommentLikes.FirstOrDefault(e => e.UserId == commentLike.UserId && e.CommentId == commentLike.CommentId);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetLikeByCommentAndUserIdAsyncShouldReturnCommentLike()
    {
        // Arrange
        var commentLike = new CommentLike { CommentId = 3, UserId = 3 };
        _ = await this.context.CommentLikes.AddAsync(commentLike);
        _ = await this.context.SaveChangesAsync();

        // Act
        var result = await this.commentLikeRepository.GetLikeByCommentAndUserIdAsyn(commentLike.CommentId, commentLike.UserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(commentLike.CommentId, result.CommentId);
        Assert.Equal(commentLike.UserId, result.UserId);
    }

    [Fact]
    public async Task GetLikeByCommentAndUserIdAsyncShouldReturnNullForInvalidIds()
    {
        // Act
        var result = await this.commentLikeRepository.GetLikeByCommentAndUserIdAsyn(999, 999);

        // Assert
        Assert.Null(result);
    }
}
