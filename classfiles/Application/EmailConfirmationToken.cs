using System.Security.Cryptography;
using System.Text;

namespace MyWarehouse.Application;

public static class EmailConfirmationToken
{
    public static string GenerateRawToken()
    {
        // 32 bytes => strong token
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Base64UrlEncode(bytes);
    }

    public static string HashToken(string rawToken)
    {
        var bytes = Encoding.UTF8.GetBytes(rawToken);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToBase64String(hashBytes);
    }

    // Base64 URL-safe encoding (no + / =)
    private static string Base64UrlEncode(byte[] bytes)
        => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}