using System.Security.Claims;
using Core.PersistenceLayer.Repositories.Auditing;
using Microsoft.AspNetCore.Http;

namespace Core.ApplicationLayer.Auditing;

/// <summary>Reads the signed-in user's NameIdentifier claim from the current request.</summary>
public class HttpContextAuditUserProvider(IHttpContextAccessor httpContextAccessor) : IAuditUserProvider
{
    public string? UserId => httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}
