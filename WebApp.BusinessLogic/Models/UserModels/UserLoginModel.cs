using System.ComponentModel.DataAnnotations;

namespace WebApp.BusinessLogic.Models.UserModels;
public class UserLoginModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}
