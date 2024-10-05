namespace WebApp.WebApi.ViewModels;

/// <summary>
/// Represents a like associated with a comment.
/// </summary>
public class CommentLikeValue
{
    /// <summary>
    /// Gets or sets the identifier of the comment that is liked.
    /// </summary>
    public int CommentId { get; set; }
}
