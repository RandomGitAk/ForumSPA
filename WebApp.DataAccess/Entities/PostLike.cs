namespace WebApp.DataAccess.Entities;
public class PostLike
{
    public int PostId { get; set; }

    public int UserId { get; set; }

    public bool IsLike { get; set; }

    public Post? Post { get; set; }

    public User? User { get; set; }
}
