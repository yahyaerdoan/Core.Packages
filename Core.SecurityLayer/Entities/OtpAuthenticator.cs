using Core.PersistenceLayer.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.SecurityLayer.Entities;

public class OtpAuthenticator : Entity<int>
{
  

    public int UserId { get; set; }
    public byte[] SecretKey { get; set; }
    public bool IsVerified { get; set; }
    public OtpAuthenticator()
    {
        SecretKey = Array.Empty<byte>();
    }
    public OtpAuthenticator(int userId, byte[] secretKey, bool isVerified)
    {
        UserId = userId;
        SecretKey = secretKey;
        IsVerified = isVerified;
    }
    public OtpAuthenticator(int id, int userId, byte[] secretKey, bool isVerified) : base(id)
    {
        UserId = userId;
        SecretKey = secretKey;
        IsVerified = isVerified;
    }
}
