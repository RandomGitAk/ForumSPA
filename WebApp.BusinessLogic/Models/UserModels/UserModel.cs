using Microsoft.AspNetCore.Http;
using WebApp.BusinessLogic.Models.RoleModels;

namespace WebApp.BusinessLogic.Models.UserModels;
public class UserModel : BaseModel
{
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public IFormFile? File { get; set; }

    public string? Image { get; set; }

    public RoleModel? Role { get; set; }

    public DateTime RegistrationDate { get; set; }
}
