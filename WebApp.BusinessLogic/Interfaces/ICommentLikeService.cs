using WebApp.BusinessLogic.Models.CommentLikeModels;

namespace WebApp.BusinessLogic.Interfaces;
public interface ICommentLikeService
{
    Task<CommentLikeModel> AddAsync(CommentLikeModel model);

    Task<CommentLikeModel> GetByIdAsync(int userId, int commentId);

    Task DeleteAsync(int userId, int commentId);
}
