using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Data;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Interfaces;

namespace WebApp.DataAccess.Repositories;
public class CommentLikeRepository : ICommentLike
{
    private readonly ApplicationContext context;

    public CommentLikeRepository(ApplicationContext context)
    {
        this.context = context;
    }

    public async Task AddAsync(CommentLike entity)
    {
        _ = await this.context.Set<CommentLike>().AddAsync(entity);
        _ = await this.context.SaveChangesAsync();
    }

    public void Delete(CommentLike entity)
    {
        _ = this.context.Set<CommentLike>().Remove(entity);
        _ = this.context.SaveChanges();
    }

    public async Task<CommentLike?> GetLikeByCommentAndUserIdAsyn(int commentId, int userId)
    {
        return await this.context.CommentLikes.FirstOrDefaultAsync(e => e.CommentId == commentId && e.UserId == userId);
    }
}
