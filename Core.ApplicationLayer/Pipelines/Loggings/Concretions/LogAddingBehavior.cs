using System.Text.Json;
using Core.ApplicationLayer.Pipelines.Loggings.Abstractions;
using Core.CrossCuttingConcernLayer.Loggings.Parameters;
using Core.CrossCuttingConcernLayer.Loggings.Serilogs.Services;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Core.ApplicationLayer.Pipelines.Loggings.Concretions;

public class LogAddingBehavior<TRequest, TResponse>(IHttpContextAccessor httpContextAccessor, BaseLoggerService baseLoggerService)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, ILogAddRequest
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        List<LogParameter> logParameters = [new() { Name = nameof(request), Type = request.GetType().Name, Value = RequestRedactor<TRequest>.Redact(request) }];

        LogDetail logDetail = new() { FullName = typeof(TRequest).FullName ?? typeof(TRequest).Name, MethodName = typeof(TRequest).Name, Parameters = logParameters, User = httpContextAccessor.HttpContext?.User.Identity?.Name ?? "?" };
        baseLoggerService.Info(JsonSerializer.Serialize(logDetail));
        return await next(cancellationToken);
    }
}
