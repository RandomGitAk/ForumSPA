using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Data;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Interfaces;

namespace WebApp.DataAccess.Repositories;
public class PostLikeRepository : IPostLike
{
    private readonly ApplicationContext context;

    public PostLikeRepository(ApplicationContext context)
    {
        this.context = context;
    }

    public async Task AddAsync(PostLike entity)
    {
        _ = await this.context.Set<PostLike>().AddAsync(entity);
        _ = await this.context.SaveChangesAsync();
    }

    public async Task UpdateAsync(PostLike entity)
    {
        _ = this.context.Set<PostLike>().Update(entity);
        _ = await this.context.SaveChangesAsync();
    }

    public void Delete(PostLike entity)
    {
        _ = this.context.Set<PostLike>().Remove(entity);
        _ = this.context.SaveChanges();
    }

    public async Task<PostLike?> GetLikeByPostAndUserIdAsyn(int postId, int userId)
    {
        return await this.context.PostLikes.FirstOrDefaultAsync(e => e.PostId == postId && e.UserId == userId);
    }
}
