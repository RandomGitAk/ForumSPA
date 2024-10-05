using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Data;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Entities.pages;
using WebApp.DataAccess.Interfaces;

namespace WebApp.DataAccess.Repositories;
public class PostRepository : BaseRepository<Post>, IPost
{
    private readonly ApplicationContext context;

    public PostRepository(ApplicationContext context)
        : base(context)
    {
        this.context = context;
    }

    public PagedList<Post> GetAllPostsWithDetails(QueryOptions options)
    {
        return new PagedList<Post>(
            this.context.Posts.AsNoTracking()
            .Include(e => e.Likes)
            .Include(e => e.Comments)
            .Include(e => e.User)
            .Include(e => e.Category)
            .OrderByDescending(e => e.Id), options);
    }

    public PagedList<Post> GetPostsByUserIdAsync(QueryOptions options, int userId)
    {
        return new PagedList<Post>(
            this.context.Posts.AsNoTracking()
           .Include(e => e.Likes)
           .Include(e => e.Comments)
           .Include(e => e.User)
           .Include(e => e.Category)
           .Where(e => e.UserId == userId)
           .OrderByDescending(e => e.PostedDate), options);
    }

    public async Task<Post?> GetPostWithDetailsByIdAsync(int id)
    {
        return await this.context.Posts.AsNoTracking()
            .Include(e => e.Likes)
            .Include(e => e.User)
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id);
    }
}
