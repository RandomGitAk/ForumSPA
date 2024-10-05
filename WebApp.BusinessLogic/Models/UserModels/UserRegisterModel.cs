using System.ComponentModel.DataAnnotations;

namespace WebApp.BusinessLogic.Models.UserModels;
public class UserRegisterModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string FirstName { get; set; } = null!;

    [Required]
    public string LastName { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = null!;
}
