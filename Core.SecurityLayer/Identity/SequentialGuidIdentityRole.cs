using Microsoft.AspNetCore.Identity;

namespace Core.SecurityLayer.Identity;

public abstract class SequentialGuidIdentityRole : IdentityRole<Guid>
{
    protected SequentialGuidIdentityRole() => Id = Guid.CreateVersion7();

    protected SequentialGuidIdentityRole(string roleName) : base(roleName) => Id = Guid.CreateVersion7();
}
