using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.BusinessLogic.Interfaces;
using WebApp.BusinessLogic.Models;
using WebApp.BusinessLogic.Models.PostModels;
using WebApp.WebApi.ViewModels;

namespace WebApp.WebApi.Controllers;

/// <summary>
/// API Controller for managing posts.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class PostsController : ControllerBase
{
    private readonly IPostService postService;
    private readonly ILogger<PostsController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PostsController"/> class.
    /// </summary>
    /// <param name="postService">The service for managing posts.</param>
    /// <param name="logger">The logger for this controller.</param>
    public PostsController(IPostService postService, ILogger<PostsController> logger)
    {
        this.postService = postService;
        this.logger = logger;
    }

    /// <summary>
    /// Gets all posts with details.
    /// </summary>
    /// <param name="paginationParams">Pagination parameters.</param>
    /// <returns>A list of posts with pagination.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<IEnumerable<PostModelPagination>> Get([FromQuery] PaginationParamsFromQueryModel paginationParams)
    {
        try
        {
            var posts = this.postService.GetAllPostsWithDetails(paginationParams);
            return this.Ok(posts);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "An unexpected error occurred while retrieving all posts.");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Gets a post by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the post.</param>
    /// <returns>The requested post with details.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PostWithDetailsModel>> GetById(int id)
    {
        try
        {
            var postWithComments = await this.postService.GetPostWithDetailsByIdAsync(id);
            if (postWithComments == null)
            {
                this.logger.LogWarning("Post with id {Id} was not found.", id);
                return this.NotFound();
            }

            return this.Ok(postWithComments);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "An unexpected error occurred while retrieving post with ID {Id}.", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Gets posts for the authenticated user.
    /// </summary>
    /// <param name="paginationParams">Pagination parameters.</param>
    /// <returns>A list of user's posts with pagination.</returns>
    [Authorize]
    [HttpGet("userPosts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<IEnumerable<PostModel>> GetUserPosts([FromQuery] PaginationParamsFromQueryModel paginationParams)
    {
        try
        {
            _ = int.TryParse(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId);
            if (userId == 0)
            {
                this.logger.LogWarning("Unauthorized access attempt for user id: {UserId}.", userId);
                return this.Unauthorized("User id not exist");
            }

            var userPosts = this.postService.GetPostsByUserId(paginationParams, userId);
            if (userPosts == null)
            {
                this.logger.LogWarning("No posts found for user id {UserId}.", userId);
                return this.NotFound();
            }

            return this.Ok(userPosts);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "An unexpected error occurred while retrieving user posts for user.");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Creates a new post.
    /// </summary>
    /// <param name="value">The post input model.</param>
    /// <returns>The created post.</returns>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Post([FromBody] PostInputAddModel value)
    {
        try
        {
            _ = int.TryParse(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId);
            if (userId == 0)
            {
                this.logger.LogWarning("Unauthorized access attempt for user id: {UserId}.", userId);
                return this.Unauthorized("User id not exist");
            }

            if (value == null)
            {
                this.logger.LogWarning("Received null PostInputAddModel.");
                return this.BadRequest();
            }

            if (!this.ModelState.IsValid)
            {
                this.logger.LogWarning("Model state is invalid: {ModelState}.", this.ModelState);
                return this.BadRequest(this.ModelState);
            }

            var postModel = await this.postService.AddAsync(new CreatePostModel
            {
                Title = value.Title,
                Content = value.Content,
                CategoryId = value.CategoryId,
                UserId = userId,
            });

            var existingPost = await this.postService.GetPostWithDetailsByIdAsync(postModel.Id);

            return this.CreatedAtAction(nameof(this.GetById), new { id = existingPost.Id }, existingPost);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "An unexpected error occurred while creating a post.");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Updates an existing post.
    /// </summary>
    /// <param name="id">The identifier of the post.</param>
    /// <param name="value">The updated post input model.</param>
    /// <returns>No content if successful.</returns>
    [Authorize(Roles = "Admin,Moderator")]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Put(int id, [FromBody] PostInputAddModel value)
    {
        try
        {
            _ = int.TryParse(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId);
            if (userId == 0)
            {
                this.logger.LogWarning("Unauthorized access attempt for user id: {UserId}.", userId);
                return this.Unauthorized("User id not exist");
            }

            if (!this.ModelState.IsValid || value == null)
            {
                this.logger.LogWarning("Model state is invalid or value is null: {ModelState}.", this.ModelState);
                return this.BadRequest(this.ModelState);
            }

            await this.postService.UpdateAsync(new CreatePostModel
            {
                Id = id,
                Title = value.Title,
                Content = value.Content,
                CategoryId = value.CategoryId,
                UserId = userId,
            });
            return this.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            this.logger.LogWarning(ex, "Post with id {Id} not found for update.", id);
            return this.NotFound();
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "An unexpected error occurred while updating post with ID {Id}.", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Increments the view count of a post.
    /// </summary>
    /// <param name="id">The identifier of the post.</param>
    /// <returns>No content if successful.</returns>
    [HttpPatch("{id}/views")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> IncrementViews(int id)
    {
        try
        {
            await this.postService.IncrementPostViews(id);
            return this.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            this.logger.LogWarning(ex, "Post with id {Id} not found when incrementing views.", id);
            return this.NotFound(ex.Message);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "An unexpected error occurred while incrementing views for post ID {Id}.", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Deletes a post by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the post.</param>
    /// <returns>No content if successful.</returns>
    [Authorize(Roles = "Admin,Moderator")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            await this.postService.DeleteAsync(id);
            return this.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            this.logger.LogWarning(ex, "Post with id {Id} not found for deletion.", id);
            return this.NotFound();
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "An unexpected error occurred while deleting post with ID {Id}.", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }
}
