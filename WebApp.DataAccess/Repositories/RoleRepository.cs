using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Data;
using WebApp.DataAccess.Entities;
using WebApp.DataAccess.Interfaces;

namespace WebApp.DataAccess.Repositories;
public class RoleRepository : BaseRepository<Role>, IRole
{
    private readonly ApplicationContext context;

    public RoleRepository(ApplicationContext context)
        : base(context)
    {
        this.context = context;
    }

    public async Task<Role?> FindByNameAsync(string roleName)
    {
        return await this.context.Roles.FirstOrDefaultAsync(e => e.Name == roleName);
    }
}
