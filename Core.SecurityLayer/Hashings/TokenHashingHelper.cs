using System.Security.Cryptography;
using System.Text;

namespace Core.SecurityLayer.Hashings;

// Refresh tokens are high-entropy random values, not user-chosen secrets, so an unsalted SHA-256
// digest is enough - no offline brute-force risk to defend against like with passwords.
public static class TokenHashingHelper
{
    public static string Hash(string rawToken)
    {
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
    }
}
