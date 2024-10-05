using WebApp.BusinessLogic.Models.PostLikeModels;

namespace WebApp.BusinessLogic.Interfaces;
public interface IPostLikeService
{
    Task<PostLikeModel> AddAsync(PostLikeModel model);

    Task<PostLikeModel> GetByIdAsync(int userId, int postId);

    Task DeleteAsync(int userId, int postId);
}
