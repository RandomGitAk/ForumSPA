using WebApp.BusinessLogic.Interfaces;
using WebApp.BusinessLogic.Services;
using WebApp.DataAccess.Interfaces;
using WebApp.DataAccess.Repositories;

namespace WebApp.WebApi;

/// <summary>
/// Extension class for registering application services.
/// </summary>
public static class ServiceRegistrationExtensions
{
    /// <summary>
    /// Adds application services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    public static void AddApplicationServices(this IServiceCollection services)
    {
        _ = services.AddTransient<ICategory, CategoryRepository>();
        _ = services.AddTransient<IComment, CommentRepository>();
        _ = services.AddTransient<ICommentLike, CommentLikeRepository>();
        _ = services.AddTransient<IPost, PostRepository>();
        _ = services.AddTransient<IPostLike, PostLikeRepository>();
        _ = services.AddTransient<IUser, UserRepository>();
        _ = services.AddTransient<IRole, RoleRepository>();

        _ = services.AddTransient<ICategoryService, CategoryService>();
        _ = services.AddTransient<ICommentService, CommentService>();
        _ = services.AddTransient<ICommentLikeService, CommentLikeService>();
        _ = services.AddTransient<IPostService, PostService>();
        _ = services.AddTransient<IPostLikeService, PostLikeService>();
        _ = services.AddTransient<IUserService, UserService>();
        _ = services.AddTransient<IRoleService, RoleService>();
    }
}
