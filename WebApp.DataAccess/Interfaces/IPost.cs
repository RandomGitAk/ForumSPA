using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Entities.pages;

namespace WebApp.DataAccess.Interfaces;
public interface IPost : IRepository<Post>
{
    PagedList<Post> GetAllPostsWithDetails(QueryOptions options);

    Task<Post?> GetPostWithDetailsByIdAsync(int id);

    PagedList<Post> GetPostsByUserIdAsync(QueryOptions options, int userId);
}
