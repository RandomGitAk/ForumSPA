using WebApp.DataAccess.Entities;

namespace WebApp.DataAccess.Interfaces;
public interface IPostLike
{
    Task<PostLike?> GetLikeByPostAndUserIdAsyn(int postId, int userId);

    Task AddAsync(PostLike entity);

    Task UpdateAsync(PostLike entity);

    void Delete(PostLike entity);
}
