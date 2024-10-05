using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.BusinessLogic.Interfaces;
using WebApp.BusinessLogic.Models.CommentModels;
using WebApp.WebApi.ViewModels;

namespace WebApp.WebApi.Controllers;

/// <summary>
/// Controller for managing comments related to posts.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class CommentsController : ControllerBase
{
    private readonly ICommentService commentService;
    private readonly ILogger<CommentsController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommentsController"/> class.
    /// </summary>
    /// <param name="commentService">The service for managing comments.</param>
    /// <param name="logger">Logger param. </param>
    public CommentsController(ICommentService commentService, ILogger<CommentsController> logger)
    {
        this.commentService = commentService;
        this.logger = logger;
    }

    /// <summary>
    /// Gets comments for a specific post by its identifier.
    /// </summary>
    /// <param name="postId">The identifier of the post.</param>
    /// <returns>A list of comments for the specified post.</returns>
    [HttpGet("posts/{postId}/comments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CommentWithUserModel>>> GetByPostId(int postId)
    {
        try
        {
            var comments = await this.commentService.GetByPostIdWithUserAsync(postId);
            if (comments == null)
            {
                this.logger.LogWarning("No comments found for post with ID: {PostId}.", postId);
                return this.NotFound();
            }

            return this.Ok(comments);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "An unexpected error occurred while retrieving comments for post ID: {PostId}.", postId);
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Gets a specific comment by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the comment.</param>
    /// <returns>The comment with the specified identifier.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CommentWithUserModel>>> GetById(int id)
    {
        try
        {
            var existingComment = await this.commentService.GetByIdAsync(id);
            if (existingComment == null)
            {
                this.logger.LogWarning("No comments found for post with ID: {Id}.", id);
                return this.NotFound();
            }

            return this.Ok(existingComment);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "An unexpected error occurred while retrieving comment ID: {Id}.", id);
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    /// <summary>
    /// Adds a new comment to a post.
    /// </summary>
    /// <param name="value">The model representing the comment to be added.</param>
    /// <returns>The created comment.</returns>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Post([FromBody] CommentInputAddModel value)
    {
        try
        {
            _ = int.TryParse(this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId);
            if (userId == 0)
            {
                this.logger.LogWarning("Unauthorized access attempt. No valid user id found.");
                return this.Unauthorized("User id not exist");
            }

            if (value == null)
            {
                this.logger.LogWarning("Bad request: Comment input is null.");
                return this.BadRequest();
            }

            if (!this.ModelState.IsValid)
            {
                this.logger.LogWarning("Bad request: Model state is invalid.");
                return this.BadRequest(this.ModelState);
            }

            var createdComment = await this.commentService.AddAsync(new CommentModel
            {
                Content = value.Content,
                PostId = value.PostId,
                UserId = userId,
                ParentCommentId = value.ParentCommentId,
            });

            var createdCommentWithUser = await this.commentService.GetByIdWithUserAsync(createdComment.Id);

            return this.CreatedAtAction(nameof(this.GetById), new { id = createdCommentWithUser.Id }, createdCommentWithUser);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            this.logger.LogError(ex, "An unexpected error occurred while adding a comment.");
            return this.StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }
}
