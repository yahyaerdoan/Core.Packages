using Core.CrossCuttingConcernLayer.ExceptionHandlings.Types.Validations;

using FluentValidation;

using MediatR;

using ValidationException = Core.CrossCuttingConcernLayer.ExceptionHandlings.Types.Validations.ValidationException;


namespace Core.ApplicationLayer.Pipelines.Validations.Concretions;

public class ValidationAddingBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validator)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ValidationContext<object> context = new(request);

        IEnumerable<ValidationExceptionModel> errors = [.. validator
            .Select(validator => validator.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(failure => failure != null)
            .GroupBy(keySelector: p => p.PropertyName, resultSelector: (propertyName, errors) => new ValidationExceptionModel
            {
                Property = propertyName,
                Errors = errors.Select(e => e.ErrorMessage)
            })];
        if (errors.Any())
            throw new ValidationException(errors);
        TResponse response = await next(cancellationToken);
        return response;
    }
}
