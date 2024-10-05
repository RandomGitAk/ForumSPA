using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.BusinessLogic.Interfaces;
using WebApp.BusinessLogic.Models;
using WebApp.BusinessLogic.Models.UserModels;

namespace WebApp.WebApi.Controllers;

/// <summary>
/// Controller responsible for user authentication and token management.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IUserService userService;
    private readonly ILogger<AuthController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="userService">The user service.</param>
    /// <param name="logger">The logger service.</param>
    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        this.userService = userService;
        this.logger = logger;
    }

    /// <summary>
    /// Logs in the user and returns the authentication token.
    /// </summary>
    /// <param name="model">The user login model.</param>
    /// <returns>Returns the tokens if login is successful.</returns>
    [HttpPost("token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] UserLoginModel model)
    {
        if (!this.ModelState.IsValid)
        {
            this.logger.LogWarning("Login attempt with invalid model state.");
            return this.BadRequest();
        }

        try
        {
            var tokensResponse = await this.userService.LoginAsync(model);
            return this.Ok(tokensResponse);
        }
        catch (ArgumentException ex)
        {
            this.logger.LogWarning(ex, "Login failed with bad request.");
            return this.BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            this.logger.LogWarning(ex, "Unauthorized login attempt with invalid credentials.");
            return this.Unauthorized("Invalid email or password.");
        }
        catch (InvalidOperationException ex)
        {
            this.logger.LogError(ex, "Invalid operation during login.");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "Unexpected error during login.");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Logs out the current user and invalidates the token.
    /// </summary>
    /// <returns>Returns success message if logout is successful.</returns>
    [Authorize]
    [HttpDelete("token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout()
    {
        try
        {
            _ = int.TryParse(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId);

            if (userId == 0)
            {
                this.logger.LogWarning("Unauthorized logout attempt: User ID not found.");
                return this.Unauthorized("User id not found.");
            }

            await this.userService.DeleteRefreshTokenAsync(userId);

            return this.Ok(new { message = "User logged out successfully." });
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "Unexpected error during logout.");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Refreshes the authentication token.
    /// </summary>
    /// <param name="request">The refresh token request model.</param>
    /// <returns>Returns the new tokens if the refresh is successful.</returns>
    [HttpPut("token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var user = await this.userService.GetUserByRefreshTokenAsync(request?.RefreshToken!);
            var tokensResponse = await this.userService.RefreshTokenAsync(user.Id, request?.RefreshToken!);
            return this.Ok(tokensResponse);
        }
        catch (UnauthorizedAccessException ex)
        {
            this.logger.LogWarning(ex, "Unauthorized token refresh attempt.");
            return this.Unauthorized();
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "Unexpected error during token refresh.");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Gets the details of the current logged-in user.
    /// </summary>
    /// <returns>Returns the user details if found.</returns>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserModel>> GetCurrentUser()
    {
        try
        {
            var userIdStr = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdStr == null)
            {
                return this.Unauthorized();
            }

            _ = int.TryParse(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId);
            var existingUser = await this.userService.GetByIdAsync(userId);

            if (existingUser == null)
            {
                return this.NotFound();
            }

            return this.Ok(existingUser);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "Unexpected error while retrieving current user.");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }
}
