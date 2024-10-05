using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.BusinessLogic.Interfaces;
using WebApp.BusinessLogic.Models.PostLikeModels;

namespace WebApp.WebApi.Controllers;

/// <summary>
/// Controller for managing post likes.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PostLikesController : ControllerBase
{
    private readonly IPostLikeService likeService;
    private readonly ILogger<PostLikesController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PostLikesController"/> class.
    /// </summary>
    /// <param name="likeService">Service for managing post likes.</param>
    /// <param name="logger">Logger for this controller.</param>
    public PostLikesController(IPostLikeService likeService, ILogger<PostLikesController> logger)
    {
        this.likeService = likeService;
        this.logger = logger;
    }

    /// <summary>
    /// Gets the post like by the specified id.
    /// </summary>
    /// <param name="id">The post ID.</param>
    /// <returns>The post like model.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PostLikeModel>> GetById(int id)
    {
        try
        {
            _ = int.TryParse(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId);
            if (userId == 0)
            {
                this.logger.LogWarning("Unauthorized access attempt: User ID not found.");
                return this.Unauthorized("User id not exist");
            }

            var existingLike = await this.likeService.GetByIdAsync(userId, id);
            if (existingLike == null)
            {
                this.logger.LogWarning("Post like not found for Post ID: {PostId}.", id);
                return this.NotFound();
            }

            return this.Ok(existingLike);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "An unexpected error occurred while retrieving post like for Post ID: {PostId}.", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Adds a new like or dislike to a post.
    /// </summary>
    /// <param name="model">The like/dislike model.</param>
    /// <returns>The action result.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Post([FromBody] LikeValuePost model)
    {
        try
        {
            _ = int.TryParse(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId);
            if (userId == 0)
            {
                return this.Unauthorized("User id not exist");
            }

            if (!this.ModelState.IsValid || model == null)
            {
                return this.BadRequest(this.ModelState);
            }

            _ = await this.likeService.AddAsync(new PostLikeModel
            {
                PostId = model.PostId,
                UserId = userId,
                IsLike = model.IsLike,
            });
            return this.CreatedAtAction(nameof(this.GetById), new { id = model.PostId }, model.PostId);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "An unexpected error occurred while adding post like for Post ID: {PostId}.", model?.PostId);
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Deletes a like or dislike from a post.
    /// </summary>
    /// <param name="id">The post ID.</param>
    /// <returns>The action result.</returns>
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
                this.logger.LogWarning("Unauthorized access attempt: User ID not found.");
                return this.Unauthorized("User id not exist");
            }

            await this.likeService.DeleteAsync(userId, id);
            return this.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            this.logger.LogWarning(ex, "Post like not found for Post ID: {Id}.", id);
            return this.NotFound();
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "An unexpected error occurred while deleting post like for Post ID: {PostId}.", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }
}
