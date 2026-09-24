using FluentValidation;
using FluentValidation.Results;
using MediatR;
using ResultHandler.Core.Abstractions;

namespace Core.ApplicationLayer.Pipelines.Validations.Concretions;

public class ValidationAddingBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IOperationResult, IResultFailureFactory<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        List<ValidationFailure> failures = [];
        foreach (IValidator<TRequest> validator in validators)
        {
            ValidationResult result = await validator.ValidateAsync(request, cancellationToken);
            failures.AddRange(result.Errors);
        }

        if (failures.Count == 0)
        {
            return await next(cancellationToken);
        }

        Dictionary<string, IReadOnlyList<string>> fieldErrors = failures
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(group => group.Key, IReadOnlyList<string> (group) => [.. group.Select(failure => failure.ErrorMessage).Distinct()]);

        return TResponse.Failure(fieldErrors);
    }
}
