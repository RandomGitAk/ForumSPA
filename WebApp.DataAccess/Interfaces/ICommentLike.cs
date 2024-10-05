using WebApp.DataAccess.Entities;

namespace WebApp.DataAccess.Interfaces;
public interface ICommentLike
{
    Task<CommentLike?> GetLikeByCommentAndUserIdAsyn(int commentId, int userId);

    Task AddAsync(CommentLike entity);

    void Delete(CommentLike entity);
}
