using System.Security.Cryptography;
using System.Text;

namespace Core.SecurityLayer.Hashings;

public class HashingHelper
{
    public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using HMACSHA512 hmacSHA512 = new();
        passwordSalt = hmacSHA512.Key;
        passwordHash = hmacSHA512.ComputeHash(Encoding.UTF8.GetBytes(password));
    }
    public static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        using HMACSHA512 hmacSHA512 = new(passwordSalt);

        byte[] computedHashCode = hmacSHA512.ComputeHash(Encoding.UTF8.GetBytes(password));
        return computedHashCode.SequenceEqual(passwordHash);
    }
}
