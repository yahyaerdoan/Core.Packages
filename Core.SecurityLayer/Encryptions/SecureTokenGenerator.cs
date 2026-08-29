using System.Buffers.Text;
using System.Security.Cryptography;

namespace Core.SecurityLayer.Encryptions;

public static class SecureTokenGenerator
{
    public static string GenerateUrlSafeToken(int byteLength = 32)
    {
        return Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(byteLength));
    }
}
