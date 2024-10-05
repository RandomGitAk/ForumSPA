using WebApp.BusinessLogic.Models.CommentModels;

namespace WebApp.BusinessLogic.Interfaces;
public interface ICommentService : ICud<CommentModel, CommentWithUserModel>, IRead<CommentModel>
{
    Task<CommentWithUserModel> GetByIdWithUserAsync(int id);

    Task<IEnumerable<CommentWithUserModel>> GetByPostIdWithUserAsync(int postId);
}
