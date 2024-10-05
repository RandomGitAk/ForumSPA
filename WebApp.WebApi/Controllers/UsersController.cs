using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.BusinessLogic.Interfaces;
using WebApp.BusinessLogic.Models;
using WebApp.BusinessLogic.Models.UserModels;
using WebApp.BusinessLogic.Validation;
using WebApp.WebApi.ViewModels;

namespace WebApp.WebApi.Controllers;

/// <summary>
/// Controller responsible for managing user-related operations.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService userService;
    private readonly ILogger<UsersController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersController"/> class.
    /// </summary>
    /// <param name="userService">The user service.</param>
    /// <param name="logger">The logger for logging events.</param>
    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        this.userService = userService;
        this.logger = logger;
    }

    /// <summary>
    /// Retrieves a paginated list of users.
    /// </summary>
    /// <param name="paginationParams">The pagination parameters.</param>
    /// <returns>Paginated list of users.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<IEnumerable<UserModelPagination>> Get([FromQuery] PaginationParamsFromQueryModel paginationParams)
    {
        try
        {
            var users = this.userService.GetAllUsersWithDetails(paginationParams);
            return this.Ok(users);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "An unexpected error occurred while retrieving users.");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Retrieves a user by their ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The user model if found.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserModel>> GetById(int id)
    {
        try
        {
            var existingUser = await this.userService.GetByIdAsync(id);
            if (existingUser == null)
            {
                this.logger.LogWarning("User with ID {UserId} not found.", id);
                return this.NotFound();
            }

            return this.Ok(existingUser);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "An unexpected error occurred while retrieving user with ID {UserId}.", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Updates the role of a user.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="updateUserRoleModel">The role update model.</param>
    /// <returns>No content if successful.</returns>
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PatchUser(int id, UpdateUserRoleModel updateUserRoleModel)
    {
        try
        {
            var user = await this.userService.GetByIdAsync(id);
            if (user == null)
            {
                this.logger.LogWarning("User with ID {UserId} not found.", id);
                return this.NotFound();
            }

            if (!this.ModelState.IsValid || updateUserRoleModel == null)
            {
                this.logger.LogWarning("Invalid model state or updateUserRoleModel is null for user ID {UserId}.", id);
                return this.BadRequest();
            }

            await this.userService.UpdateUserRoleAsync(user.Id, updateUserRoleModel.RoleId);

            return this.NoContent();
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "An unexpected error occurred while updating role for user ID {UserId}.", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="userModel">The user registration model.</param>
    /// <returns>Confirmation of successful registration.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] UserRegisterModel userModel)
    {
        try
        {
            if (!this.ModelState.IsValid)
            {
                this.logger.LogWarning("Invalid model state during user registration.");
                return this.BadRequest();
            }

            bool result = await this.userService.RegisterUserAsync(userModel);
            if (!result)
            {
                this.logger.LogWarning("User with email {Email} already exists.", userModel?.Email);
                return this.Conflict(new { message = "User with this email already exists." });
            }

            return this.Ok(new { message = "User registered successfully!" });
        }
        catch (RoleNotFoundException ex)
        {
            this.logger.LogWarning(ex, "Role not found during user registration.");
            return this.BadRequest(new { message = ex.Message });
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "An unexpected error occurred during user registration.");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Updates the current user.
    /// </summary>
    /// <param name="userUpdateModel">The user update model.</param>
    /// <returns>No content if successful.</returns>
    [Authorize]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateUser([FromForm] UserUpdateModel userUpdateModel)
    {
        try
        {
            _ = int.TryParse(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId);

            if (userId == 0)
            {
                this.logger.LogWarning("Unauthorized access attempt by user without ID.");
                return this.Unauthorized("User id not found.");
            }

            if (!this.ModelState.IsValid)
            {
                this.logger.LogWarning("Invalid model state during user update for user ID {UserId}.", userId);
                return this.BadRequest(this.ModelState);
            }

            await this.userService.UpdateAsync(new UserModel
            {
                Id = userId,
                FirstName = userUpdateModel?.FirstName!,
                LastName = userUpdateModel?.LastName!,
                File = userUpdateModel?.File,
            });
            return this.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            this.logger.LogWarning(ex, "User with ID not found during update.");
            return this.NotFound(ex.Message);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "An unexpected error occurred while updating user ID.");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Deletes a user by their ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>No content if successful.</returns>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            await this.userService.DeleteAsync(id);
            return this.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            this.logger.LogWarning(ex, "Attempted to delete non-existing user with ID {UserId}.", id);
            return this.NotFound();
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "An unexpected error occurred while deleting user ID {UserId}.", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }
}
