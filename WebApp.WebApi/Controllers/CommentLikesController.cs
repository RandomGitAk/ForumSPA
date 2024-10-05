using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.BusinessLogic.Interfaces;
using WebApp.BusinessLogic.Models.CommentLikeModels;
using WebApp.BusinessLogic.Models.PostLikeModels;
using WebApp.WebApi.ViewModels;

namespace WebApp.WebApi.Controllers;

/// <summary>
/// Controller for managing comment likes.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CommentLikesController : ControllerBase
{
    private readonly ICommentLikeService likeService;
    private readonly ILogger<CommentLikesController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommentLikesController"/> class.
    /// </summary>
    /// <param name="likeService">The service for managing comment likes.</param>
    /// <param name="logger">The logger.</param>
    public CommentLikesController(ICommentLikeService likeService, ILogger<CommentLikesController> logger)
    {
        this.likeService = likeService;
        this.logger = logger;
    }

    /// <summary>
    /// Gets a specific like by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the like.</param>
    /// <returns>The <see cref="PostLikeModel"/> associated with the specified identifier.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CommentLikeModel>> GetById(int id)
    {
        try
        {
            _ = int.TryParse(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId);
            if (userId == 0)
            {
                this.logger.LogWarning("Unauthorized access attempt. No valid user id found in claims.");
                return this.Unauthorized("User id not exist");
            }

            var existingLike = await this.likeService.GetByIdAsync(userId, id);
            if (existingLike == null)
            {
                this.logger.LogWarning("Like not found for comment {CommentId} by user {UserId}", id, userId);
                return this.NotFound();
            }

            return this.Ok(existingLike);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "Unexpected error occurred while retrieving like for comment {CommentId}", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Creates a new like for a comment.
    /// </summary>
    /// <param name="model">The model containing the comment ID to like.</param>
    /// <returns>A created response with the identifier of the liked comment.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Post([FromBody] CommentLikeValue model)
    {
        try
        {
            _ = int.TryParse(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId);
            if (userId == 0)
            {
                this.logger.LogWarning("Unauthorized access attempt. No valid user id found in claims.");
                return this.Unauthorized("User id not exist");
            }

            if (!this.ModelState.IsValid || model == null)
            {
                this.logger.LogWarning("Invalid model state for adding like to comment {CommentId}", model?.CommentId);
                return this.BadRequest(this.ModelState);
            }

            _ = await this.likeService.AddAsync(new CommentLikeModel
            {
                CommentId = model.CommentId,
                UserId = userId,
            });
            return this.CreatedAtAction(nameof(this.GetById), new { id = model.CommentId }, model.CommentId);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "Error occurred while adding a like for comment {CommentId}", model?.CommentId);
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Deletes a like for a comment.
    /// </summary>
    /// <param name="id">The identifier of the like to delete.</param>
    /// <returns>No content response if successful.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            _ = int.TryParse(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId);
            if (userId == 0)
            {
                this.logger.LogWarning("Unauthorized access attempt. No valid user id found in claims.");
                return this.Unauthorized("User id not exist");
            }

            await this.likeService.DeleteAsync(userId, id);
            return this.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            this.logger.LogWarning(ex, "Attempt to delete like failed because it does not exist. Comment {CommentId}", id);
            return this.NotFound();
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "Error occurred while deleting like for comment {CommentId}", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }
}
