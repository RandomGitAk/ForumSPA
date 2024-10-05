using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Data;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Interfaces;

namespace WebApp.DataAccess.Repositories;
public class CommentRepository : BaseRepository<Comment>, IComment
{
    private readonly ApplicationContext context;

    public CommentRepository(ApplicationContext context)
        : base(context)
    {
        this.context = context;
    }

    public override async Task<Comment?> GetByIdAsync(int id)
    {
        return await this.context.Comments.Include(e => e.User).FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Comment>> GetByPostIdAsync(int postId)
    {
        return await this.context.Comments.Include(e => e.User)
            .Include(e => e.Likes)
            .Where(e => e.PostId == postId && e.ParentCommentId == null)
            .Include(c => c.Replies!)
               .ThenInclude(r => r.User)
            .Include(c => c.Replies!)
               .ThenInclude(r => r.Likes)
            .OrderByDescending(e => e.CommentDate)
            .ToListAsync();
    }
}
