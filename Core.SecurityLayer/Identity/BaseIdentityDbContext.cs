using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Core.SecurityLayer.Identity;

// TKey left open so consumers can pick Guid, int, or string.
public abstract class BaseIdentityDbContext<TUser, TRole, TKey>(DbContextOptions options)
    : IdentityDbContext<TUser, TRole, TKey>(options)
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}
