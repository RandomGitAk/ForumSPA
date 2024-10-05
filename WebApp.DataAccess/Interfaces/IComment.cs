using WebApp.DataAccess.Entities;

namespace WebApp.DataAccess.Interfaces;
public interface IComment : IRepository<Comment>
{
    Task<IEnumerable<Comment>> GetByPostIdAsync(int postId);
}
