using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace WebApp.DataAccess.Helpers;
public static class AuthOptions
{
    public const string ISSUER = "MyAuthServer";
    public const string AUDIENCE = "MyAuthClient";
    private const string KEY = "YourSuperSecretKey1234567890123456";

    public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
}
