namespace Core.SecurityLayer.Authorization;

public interface IPermissionCatalog
{
    IReadOnlyDictionary<string, string[]> PermissionsByModule { get; }

    IReadOnlyList<string> AllPermissions { get; }
}
