using WebApp.BusinessLogic.Models;
using WebApp.BusinessLogic.Models.UserModels;

namespace WebApp.BusinessLogic.Interfaces;
public interface IUserService : ICrud<UserModel>
{
    Task<TokenResponseModel> LoginAsync(UserLoginModel userModel);

    Task<bool> RegisterUserAsync(UserRegisterModel userModel, string roleName = "User");

    Task<TokenResponseModel> RefreshTokenAsync(int userId, string refreshToken);

    Task DeleteRefreshTokenAsync(int userId);

    Task<UserModel> GetUserByRefreshTokenAsync(string refreshToken);

    Task UpdateUserRoleAsync(int userId, int userRoleId);

    UserModelPagination GetAllUsersWithDetails(PaginationParamsFromQueryModel paginationParams);
}
