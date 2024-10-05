using WebApp.DataAccess.Entities;

namespace WebApp.DataAccess.Interfaces;
public interface IRole : IRepository<Role>
{
    Task<Role?> FindByNameAsync(string roleName);
}
