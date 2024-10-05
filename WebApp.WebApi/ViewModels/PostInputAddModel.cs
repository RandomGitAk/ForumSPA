namespace WebApp.WebApi.ViewModels;

/// <summary>
/// Represents the input model for adding a new post.
/// </summary>
public class PostInputAddModel
{
    /// <summary>
    /// Gets or sets the title of the post.
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Gets or sets the content of the post.
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// Gets or sets the category identifier for the post.
    /// </summary>
    public int CategoryId { get; set; }
}
