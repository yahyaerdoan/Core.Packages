using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Base;
using ResultHandler.Facade;

namespace Core.ApplicationLayer.Extensions;

internal static class ResponseErrorCodeExtensions
{
    /// <summary>Gives a pipeline's failed response an error code; a response type that is not an <see cref="OperationResult"/> is returned unchanged.</summary>
    internal static TResponse WithErrorCode<TResponse>(this TResponse response, string code)
        where TResponse : IOperationResult
        => response is OperationResult result ? (TResponse)(IOperationResult)result.WithCode(code) : response;
}
