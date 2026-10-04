using Core.ApplicationLayer.Requests.Page;
using Core.PersistenceLayer.Dynamics.Extensions;
using FluentValidation;

namespace Core.ApplicationLayer.Requests.Lists;

/// <summary>
/// One validator for every <see cref="IDynamicListRequest"/>: page bounds plus the filter and sort rules against the request's
/// <see cref="IDynamicListRequest.QueryableFields"/>. Register once as an open generic: AddTransient(typeof(IValidator&lt;&gt;), typeof(DynamicListRequestValidator&lt;&gt;)).
/// </summary>
public class DynamicListRequestValidator<TRequest> : AbstractValidator<TRequest>
    where TRequest : IDynamicListRequest
{
    public DynamicListRequestValidator()
    {
        _ = RuleFor(r => r.PageRequest).NotNull().SetValidator(new PageRequestValidator());
        _ = RuleFor(r => r.DynamicQuery).Custom((query, context) =>
        {
            foreach (string error in DynamicQueryRules.Validate(query, TRequest.QueryableFields))
            {
                context.AddFailure(nameof(IDynamicListRequest.DynamicQuery), error);
            }
        });
    }
}
