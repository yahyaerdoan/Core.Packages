using Core.PersistenceLayer.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.SecurityLayer.Entities;

public class OneTimePasswordAuthenticator : Entity<int>
{
    public int UserId { get; set; }
    public byte[] SecretKey { get; set; }
    public bool IsVerified { get; set; }
    public virtual User User { get; set; } = null!;
    public OneTimePasswordAuthenticator()
    {
        SecretKey = Array.Empty<byte>();
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
