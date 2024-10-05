namespace WebApp.WebApi.ViewModels;

/// <summary>
/// Model for adding a new comment.
/// </summary>
public class CommentInputAddModel
{
    /// <summary>
    /// Gets or sets the content of the comment.
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// Gets or sets the identifier of the post to which the comment belongs.
    /// </summary>
    public int PostId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the parent comment, if this is a reply to another comment.
    /// </summary>
    public int? ParentCommentId { get; set; }
}
