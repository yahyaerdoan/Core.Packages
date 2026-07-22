using Core.PersistenceLayer.Repositories.Entities;

namespace Core.SecurityLayer.Entities;

public class OneTimePasswordAuthenticator : Entity<int>
{
    public int UserId { get; set; }
    public byte[] SecretKey { get; set; }
    public bool IsVerified { get; set; }
    public virtual User User { get; set; } = null!;
    public OneTimePasswordAuthenticator()
    {
        SecretKey = [];
    }
    public OneTimePasswordAuthenticator(int userId, byte[] secretKey, bool isVerified)
    {
        UserId = userId;
        SecretKey = secretKey;
        IsVerified = isVerified;
    }
    public OneTimePasswordAuthenticator(int id, int userId, byte[] secretKey, bool isVerified) : base(id)
    {
        UserId = userId;
        SecretKey = secretKey;
        IsVerified = isVerified;
    }
}
