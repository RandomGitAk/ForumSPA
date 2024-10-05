namespace WebApp.DataAccess.Entities;
public class User : BaseEntity
{
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Image { get; set; } = null!;

    public string HashedPasssword { get; set; } = null!;

    public string Salt { get; set; } = null!;

    public int RoleId { get; set; }

    public DateTime RegistrationDate { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryDate { get; set; }

    public Role? Role { get; set; }

    public IEnumerable<CommentLike>? CommentLikes { get; set; }

    public IEnumerable<PostLike>? PostLikes { get; set; }

    public IEnumerable<Post>? Posts { get; set; }

    public IEnumerable<Comment>? Comments { get; set; }
}
