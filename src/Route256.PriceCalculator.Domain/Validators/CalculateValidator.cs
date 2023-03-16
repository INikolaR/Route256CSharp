using FluentValidation;
using Route256.PriceCalculator.Domain.Models.PriceCalculator;

namespace Route256.PriceCalculator.Domain.Validators;

internal sealed class CalculateValidator: AbstractValidator<IReadOnlyCollection<GoodModel>>
{
    public CalculateValidator()
    { 
        RuleForEach(x => x)
            .SetValidator(new GoodPropertiesValidator());
    }
}