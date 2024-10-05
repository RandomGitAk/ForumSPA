using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Data;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Repositories;
using Xunit;

namespace Forum.Tests.DataTests;
public class PostLikeRepositoryTests
{
    private readonly ApplicationContext context;
    private readonly PostLikeRepository postLikeRepository;

    public PostLikeRepositoryTests()
    {
        // Set up an in-memory database
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .EnableSensitiveDataLogging()
            .Options;

        this.context = new ApplicationContext(options);
        this.postLikeRepository = new PostLikeRepository(this.context);
    }

    [Fact]
    public async Task AddAsyncShouldAddPostLike()
    {
        // Arrange
        var postLike = new PostLike { PostId = 1, UserId = 1, IsLike = true };

        // Act
        await this.postLikeRepository.AddAsync(postLike);

        // Assert
        var result = await this.context.PostLikes.FirstOrDefaultAsync(e => e.UserId == postLike.UserId && e.PostId == postLike.PostId);
        Assert.NotNull(result);
        Assert.Equal(1, result!.PostId);
        Assert.Equal(1, result.UserId);
        Assert.True(result.IsLike);
    }

    [Fact]
    public async Task UpdateAsyncShouldUpdatePostLike()
    {
        // Arrange
        var postLike = new PostLike { PostId = 2, UserId = 2, IsLike = false };
        await this.postLikeRepository.AddAsync(postLike);

        // Update like status
        postLike.IsLike = true;

        // Act
        await this.postLikeRepository.UpdateAsync(postLike);

        // Assert
        var result = await this.context.PostLikes.FirstOrDefaultAsync(e => e.UserId == postLike.UserId && e.PostId == postLike.PostId);
        Assert.NotNull(result);
        Assert.True(result!.IsLike);
    }

    [Fact]
    public async Task DeleteShouldRemovePostLike()
    {
        // Arrange
        var postLike = new PostLike { PostId = 3, UserId = 3, IsLike = true };
        await this.postLikeRepository.AddAsync(postLike);

        // Act
        this.postLikeRepository.Delete(postLike);

        // Assert
        var result = await this.context.PostLikes.FirstOrDefaultAsync(e => e.UserId == postLike.UserId && e.PostId == postLike.PostId);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetLikeByPostAndUserIdAsynShouldReturnPostLike()
    {
        // Arrange
        var postLike = new PostLike { PostId = 4, UserId = 4, IsLike = true };
        await this.postLikeRepository.AddAsync(postLike);

        // Act
        var result = await this.postLikeRepository.GetLikeByPostAndUserIdAsyn(postLike.PostId, postLike.UserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(postLike.PostId, result!.PostId);
        Assert.Equal(postLike.UserId, result.UserId);
    }

    [Fact]
    public async Task GetLikeByPostAndUserIdAsynShouldReturnNullWhenNotFound()
    {
        // Act
        var result = await this.postLikeRepository.GetLikeByPostAndUserIdAsyn(1, 1);

        // Assert
        Assert.Null(result);
    }
}
