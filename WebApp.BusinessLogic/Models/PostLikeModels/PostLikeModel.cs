namespace WebApp.BusinessLogic.Models.PostLikeModels;
public class PostLikeModel
{
    public int PostId { get; set; }

    public int UserId { get; set; }

    public bool IsLike { get; set; }
}
