namespace WebApp.DataAccess.Entities;
public class CommentLike
{
    public int UserId { get; set; }

    public int CommentId { get; set; }

    public User? User { get; set; }

    public Comment? Comment { get; set; }
}
