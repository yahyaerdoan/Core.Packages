using FluentValidation;

namespace Core.ApplicationLayer.Requests.Page;

/// <summary>Caps the page size so a caller can't pull a whole table through one request.</summary>
public class PageRequestValidator : AbstractValidator<PageRequest>
{
    public const int MaxPageSize = 100;

    public PageRequestValidator()
    {
        _ = RuleFor(p => p.PageIndex).GreaterThanOrEqualTo(0).WithMessage("Page index must be {ComparisonValue} or greater.");
        _ = RuleFor(p => p.PageSize).InclusiveBetween(1, MaxPageSize).WithMessage("Page size must be between {From} and {To}.");
    }
}
