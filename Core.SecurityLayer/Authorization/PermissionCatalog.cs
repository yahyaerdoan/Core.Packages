namespace Core.SecurityLayer.Authorization;

public class PermissionCatalog(IReadOnlyDictionary<string, string[]> permissionsByModule) : IPermissionCatalog
{
    public IReadOnlyDictionary<string, string[]> PermissionsByModule { get; } = permissionsByModule;

    public IReadOnlyList<string> AllPermissions { get; } = [.. permissionsByModule.Values.SelectMany(x => x)];
}
