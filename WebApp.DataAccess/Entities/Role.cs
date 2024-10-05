namespace WebApp.DataAccess.Entities;
public class Role : BaseEntity
{
    public string Name { get; set; } = null!;

    public IEnumerable<User>? Users { get; set; }
}
