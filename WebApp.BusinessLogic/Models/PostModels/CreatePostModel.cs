namespace WebApp.BusinessLogic.Models.PostModels;
public class CreatePostModel : BaseModel
{
    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public int CategoryId { get; set; }

    public int UserId { get; set; }
}
