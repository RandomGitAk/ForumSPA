namespace WebApp.WebApi.ViewModels;

/// <summary>
/// Model for adding a new category.
/// </summary>
public class CategoryInputAddModel
{
    /// <summary>
    /// Gets or sets the name of the category.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the description of the category.
    /// </summary>
    public string Description { get; set; } = null!;
}
