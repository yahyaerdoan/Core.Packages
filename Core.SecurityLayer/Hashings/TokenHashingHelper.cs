using System.Security.Cryptography;
using System.Text;

namespace Core.SecurityLayer.Hashings;

// Refresh tokens are high-entropy random values (not user-chosen secrets), so a fast, unsalted
// SHA-256 digest is sufficient here — unlike passwords, there is no offline dictionary/brute-force
// risk to defend against with a slow KDF; the risk being mitigated is a stolen DB row/backup
// yielding a directly usable bearer credential.
public static class TokenHashingHelper
{
    public static string Hash(string rawToken) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}
