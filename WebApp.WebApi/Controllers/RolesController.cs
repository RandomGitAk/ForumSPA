using Microsoft.AspNetCore.Mvc;
using WebApp.BusinessLogic.Interfaces;
using WebApp.BusinessLogic.Models.RoleModels;

namespace WebApp.WebApi.Controllers;

/// <summary>
/// API Controller for managing roles.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class RolesController : ControllerBase
{
    private readonly IRoleService roleService;
    private readonly ILogger<RolesController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RolesController"/> class.
    /// </summary>
    /// <param name="roleService">The role service to interact with role data.</param>
    /// <param name="logger">The logger for logging events.</param>
    public RolesController(IRoleService roleService, ILogger<RolesController> logger)
    {
        this.roleService = roleService;
        this.logger = logger;
    }

    /// <summary>
    /// Gets all roles.
    /// </summary>
    /// <returns>A list of roles.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<RoleModel>>> Get()
    {
        try
        {
            var roles = await this.roleService.GetAllAsync();
            return this.Ok(roles);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "An unexpected error occurred while retrieving all roles.");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }
}
