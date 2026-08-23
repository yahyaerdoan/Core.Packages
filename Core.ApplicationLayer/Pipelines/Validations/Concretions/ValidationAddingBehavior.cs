using FluentValidation;
using MediatR;
using ResultHandler.Core.Abstractions;

namespace Core.ApplicationLayer.Pipelines.Validations.Concretions;

public class ValidationAddingBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validator) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IOperationResult, IFieldFailureFactory<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ValidationContext<object> context = new(request);

        Dictionary<string, IReadOnlyList<string>> fieldErrors = validator.Select(v => v.Validate(context)).SelectMany(result => result.Errors)
            .Where(failure => failure != null)
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(group => group.Key, IReadOnlyList<string> (group) => [.. group.Select(failure => failure.ErrorMessage)]);

        return fieldErrors.Count > 0 ? TResponse.Failure(fieldErrors) : await next(cancellationToken);
    }
}
