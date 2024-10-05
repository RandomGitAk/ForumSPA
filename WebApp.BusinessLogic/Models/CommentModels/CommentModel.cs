namespace WebApp.BusinessLogic.Models.CommentModels;
public class CommentModel : BaseModel
{
    public string Content { get; set; } = null!;

    public int PostId { get; set; }

    public int UserId { get; set; }

    public int? ParentCommentId { get; set; }
}
