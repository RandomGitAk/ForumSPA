namespace WebApp.DataAccess.Entities;
public class Comment : BaseEntity
{
    public string Content { get; set; } = null!;

    public DateTime CommentDate { get; set; }

    public int PostId { get; set; }

    public int UserId { get; set; }

    public int? ParentCommentId { get; set; }

    public Comment? ParentComment { get; set; }

    public IEnumerable<Comment>? Replies { get; set; }

    public Post? Post { get; set; }

    public User? User { get; set; }

    public IEnumerable<CommentLike>? Likes { get; set; }
}
