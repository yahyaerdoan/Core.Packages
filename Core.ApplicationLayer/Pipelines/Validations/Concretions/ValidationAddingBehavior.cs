using FluentValidation;

using MediatR;

using ResultHandler.Core.Abstractions;

namespace Core.ApplicationLayer.Pipelines.Validations.Concretions;

public class ValidationAddingBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validator)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IOperationResult, IResultFailureFactory<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ValidationContext<object> context = new(request);

        string[] errors = [.. validator
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(failure => failure != null)
            .Select(failure => $"{failure.PropertyName}: {failure.ErrorMessage}")];

        if (errors.Length > 0)
            return TResponse.Failure(errors);

        return await next(cancellationToken);
    }
}
