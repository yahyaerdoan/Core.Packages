namespace Core.ApplicationLayer.Pipelines.Authorizations.Abstractions;

public interface ISecureAddRequest
{
    public string[] Roles { get; }
}
