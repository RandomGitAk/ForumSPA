using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Entities.pages;

namespace WebApp.DataAccess.Interfaces;
public interface IUser : IRepository<User>
{
    Task<User?> GetUserByEmailAsync(string email);

    Task<bool> IsUserExistsAsync(string email);

    Task<string?> GetUserSaltAsync(string email);

    Task<bool> VerifyUserAsync(User user);

    Task StoreRefreshTokenAsync(int userId, string refreshToken, DateTime expiryDate);

    Task DeleteRefreshTokenAsync(int userId);

    Task<bool> IsRefreshTokenValidAsync(int userId, string refreshToken);

    Task<User?> GetUserByRefreshTokenAsync(string refreshToken);

    PagedList<User> GetUsersPaged(QueryOptions options);
}
