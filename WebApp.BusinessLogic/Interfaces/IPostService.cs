using WebApp.BusinessLogic.Models;
using WebApp.BusinessLogic.Models.PostModels;

namespace WebApp.BusinessLogic.Interfaces;
public interface IPostService : ICud<CreatePostModel, PostModel>, IRead<PostModel>
{
    PostModelPagination GetAllPostsWithDetails(PaginationParamsFromQueryModel paginationParams);

    Task<PostWithDetailsModel> GetPostWithDetailsByIdAsync(int id);

    PostModelPagination GetPostsByUserId(PaginationParamsFromQueryModel paginationParams, int userId);

    Task IncrementPostViews(int postId);
}
