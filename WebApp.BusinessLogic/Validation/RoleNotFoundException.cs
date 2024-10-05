namespace WebApp.BusinessLogic.Validation;
public class RoleNotFoundException : Exception
{
    public RoleNotFoundException()
    {
    }

    public RoleNotFoundException(string roleName)
       : base($"Role '{roleName}' not found.")
    {
    }

    public RoleNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
