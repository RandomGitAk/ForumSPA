using WebApp.BusinessLogic.Models.UserModels;

namespace WebApp.BusinessLogic.Models.PostModels;
public class PostModel : BaseModel
{
    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public int Views { get; set; }

    public DateTime PostedDate { get; set; }

    public int CountLikes { get; set; }

    public int CountComments { get; set; }

    public string CategoryName { get; set; } = null!;

    public UserModel User { get; set; } = null!;
}
