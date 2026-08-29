namespace Core.ApplicationLayer.Pipelines.Authorizations.Abstractions;

public interface ISecureAddRequest
{
    string[] Roles { get; }
}
