namespace WebApp.DataAccess.Entities;
public class Category : BaseEntity
{
    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public IEnumerable<Post>? Posts { get; set; }
}
