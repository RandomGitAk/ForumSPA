using System.Security.Cryptography;

namespace WebApp.DataAccess.Helpers;
public static class SecurityHelper
{
    public static string GenerateSalt(int nSalt)
    {
        var saltBytes = new byte[nSalt];
        RandomNumberGenerator.Fill(saltBytes);

        return Convert.ToBase64String(saltBytes);
    }

    public static string HashPassword(string password, string salt, int nIterations, int nHash)
    {
        var saltBytes = Convert.FromBase64String(salt);
        using var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, saltBytes, nIterations, HashAlgorithmName.SHA256);
        return Convert.ToBase64String(rfc2898DeriveBytes.GetBytes(nHash));
    }
}
