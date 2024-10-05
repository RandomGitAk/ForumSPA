using System.ComponentModel.DataAnnotations;

namespace WebApp.WebApi.ViewModels;

/// <summary>
/// Model for updating user information.
/// </summary>
public class UserUpdateModel
{
    /// <summary>
    /// Gets or sets the user's first name.
    /// </summary>
    [Required]
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the user's last name.
    /// </summary>
    [Required]
    public string LastName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the file to upload (optional).
    /// </summary>
    public IFormFile? File { get; set; }
}
