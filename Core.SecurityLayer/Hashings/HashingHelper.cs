using System.Security.Cryptography;
using System.Text;

namespace Core.SecurityLayer.Hashings;

public class HashingHelper
{
    // OWASP (2023) minimum for PBKDF2-HMAC-SHA256.
    private const int Pbkdf2Iterations = 210_000;
    private const int Pbkdf2SaltSize = 16;
    private const int Pbkdf2KeySize = 32;

    // HMACSHA512's auto-generated key is always exactly this many bytes — used purely to tell a
    // legacy hash apart from a PBKDF2 one (whose salt is always Pbkdf2SaltSize) without needing an
    // extra "algorithm" column. New PBKDF2 salts can never collide with this length.
    private const int LegacyHmacSha512SaltSize = 128;

    public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        passwordSalt = RandomNumberGenerator.GetBytes(Pbkdf2SaltSize);
        passwordHash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), passwordSalt, Pbkdf2Iterations, HashAlgorithmName.SHA256, Pbkdf2KeySize);
    }

    public static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        byte[] computedHashCode = IsLegacyHash(passwordSalt)
            ? ComputeLegacyHmacSha512Hash(password, passwordSalt)
            : Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), passwordSalt, Pbkdf2Iterations, HashAlgorithmName.SHA256, passwordHash.Length);

        // Constant-time comparison — SequenceEqual short-circuits on the first mismatched byte,
        // which leaks timing information an attacker could use to guess the hash byte-by-byte.
        return CryptographicOperations.FixedTimeEquals(computedHashCode, passwordHash);
    }

    /// <summary>
    /// True when this salt/hash pair was produced by the legacy HMACSHA512 scheme. Callers should
    /// re-hash the password with <see cref="CreatePasswordHash"/> on next successful login so
    /// existing accounts migrate to PBKDF2 without ever forcing a password reset.
    /// </summary>
    public static bool IsLegacyHash(byte[] passwordSalt) => passwordSalt.Length == LegacyHmacSha512SaltSize;

    private static byte[] ComputeLegacyHmacSha512Hash(string password, byte[] passwordSalt)
    {
        using HMACSHA512 hmacSha512 = new(passwordSalt);
        return hmacSha512.ComputeHash(Encoding.UTF8.GetBytes(password));
    }
}
