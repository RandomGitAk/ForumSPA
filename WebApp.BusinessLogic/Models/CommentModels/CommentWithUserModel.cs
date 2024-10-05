using WebApp.BusinessLogic.Models.UserModels;

namespace WebApp.BusinessLogic.Models.CommentModels;
public class CommentWithUserModel : BaseModel
{
    public string Content { get; set; } = null!;

    public DateTime CommentDate { get; set; }

    public UserModel User { get; set; } = null!;

    public int CountLikes { get; set; }

    public IEnumerable<CommentWithUserModel> Replies { get; set; } =
        [];
}
