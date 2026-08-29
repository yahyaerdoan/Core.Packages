using Microsoft.AspNetCore.Identity;

namespace Core.SecurityLayer.Identity;

// Client-side sequential Guid — avoids clustered-index fragmentation from random (v4) inserts.
public abstract class SequentialGuidIdentityUser : IdentityUser<Guid>
{
    protected SequentialGuidIdentityUser()
    {
        Id = Guid.CreateVersion7();
    }

    protected SequentialGuidIdentityUser(string userName) : base(userName)
    {
        Id = Guid.CreateVersion7();
    }
}
