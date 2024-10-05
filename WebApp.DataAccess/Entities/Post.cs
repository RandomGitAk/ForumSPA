namespace WebApp.DataAccess.Entities;
public class Post : BaseEntity
{
    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public int Views { get; set; }

    public DateTime PostedDate { get; set; }

    public int UserId { get; set; }

    public int CategoryId { get; set; }

    public User? User { get; set; }

    public Category? Category { get; set; }

    public IEnumerable<Comment>? Comments { get; set; }

    public IEnumerable<PostLike>? Likes { get; set; }
}
