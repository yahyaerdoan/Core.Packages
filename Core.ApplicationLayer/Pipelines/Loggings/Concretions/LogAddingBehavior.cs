using Core.ApplicationLayer.Pipelines.Loggings.Abstractions;
using Core.CrossCuttingConcernLayer.Loggings.Parameters;
using Core.CrossCuttingConcernLayer.Loggings.Serilogs.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.ApplicationLayer.Pipelines.Loggings.Concretions;

public class LogAddingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, ILogAddRequest
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly BaseLoggerService _baseLoggerService;

    public LogAddingBehavior(IHttpContextAccessor httpContextAccessor, BaseLoggerService baseLoggerService)
    {
        _httpContextAccessor = httpContextAccessor;
        _baseLoggerService = baseLoggerService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        List<LogParameter> logParameters = new() { new() { Type = request.GetType().Name, Value = request } };

        LogDetail logDetail = new() { MethodName = next.Method.Name, Parameters = logParameters, User = _httpContextAccessor.HttpContext.User.Identity?.Name ?? "?" };
        _baseLoggerService.Info(JsonSerializer.Serialize(logDetail));
        return await next();
    }
}
