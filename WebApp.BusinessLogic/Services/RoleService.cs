using AutoMapper;
using WebApp.BusinessLogic.Interfaces;
using WebApp.BusinessLogic.Models.RoleModels;
using WebApp.DataAccess.Interfaces;

namespace WebApp.BusinessLogic.Services;
public class RoleService : IRoleService
{
    private readonly IRole roleRepository;
    private readonly IMapper mapper;

    public RoleService(IRole roleRepository, IMapper mapper)
    {
        this.roleRepository = roleRepository;
        this.mapper = mapper;
    }

    public async Task<IEnumerable<RoleModel>> GetAllAsync()
    {
        var roles = await this.roleRepository.GetAllAsync();
        return this.mapper.Map<IEnumerable<RoleModel>>(roles);
    }

    public async Task<RoleModel> GetByIdAsync(int id)
    {
        var role = await this.roleRepository.GetByIdAsync(id);
        return this.mapper.Map<RoleModel>(role);
    }
}
