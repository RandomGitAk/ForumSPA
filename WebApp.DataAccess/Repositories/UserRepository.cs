using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Data;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Entities.pages;
using WebApp.DataAccess.Interfaces;

namespace WebApp.DataAccess.Repositories;
public class UserRepository : BaseRepository<User>, IUser
{
    private readonly ApplicationContext context;

    public UserRepository(ApplicationContext context)
        : base(context)
    {
        this.context = context;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await this.context.Users.Include(e => e.Role).FirstOrDefaultAsync(e => e.Email == email);
    }

    public override async Task<User?> GetByIdAsync(int id)
    {
        return await this.context.Users.Include(e => e.Role).FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<string?> GetUserSaltAsync(string email)
    {
        return await this.context.Users.Where(e => e.Email == email).Select(e => e.Salt).FirstOrDefaultAsync();
    }

    public async Task<bool> IsUserExistsAsync(string email)
    {
        return await this.context.Users.AnyAsync(e => e.Email == email);
    }

    public async Task<bool> VerifyUserAsync(User user)
    {
        return await this.context.Users.AnyAsync(e => e.Email == user.Email && e.HashedPasssword == user.HashedPasssword);
    }

    public async Task StoreRefreshTokenAsync(int userId, string refreshToken, DateTime expiryDate)
    {
        var user = await this.GetByIdAsync(userId);
        if (user != null)
        {
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryDate = expiryDate;

            _ = this.context.Users.Update(user);
            _ = await this.context.SaveChangesAsync();
        }
    }

    public async Task DeleteRefreshTokenAsync(int userId)
    {
        var user = await this.GetByIdAsync(userId);

        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiryDate = null;

            _ = await this.context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsRefreshTokenValidAsync(int userId, string refreshToken)
    {
        var user = await this.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        return user.RefreshToken == refreshToken && user.RefreshTokenExpiryDate > DateTime.UtcNow;
    }

    public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
    {
        return await this.context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
    }

    public PagedList<User> GetUsersPaged(QueryOptions options)
    {
        return new PagedList<User>(
            this.context.Users.Include(e => e.Role).AsNoTracking()
          .OrderByDescending(e => e.Id), options);
    }
}
