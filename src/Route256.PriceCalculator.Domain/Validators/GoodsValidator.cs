using FluentValidation;
using Route256.PriceCalculator.Domain.Models.PriceCalculator;

namespace Route256.PriceCalculator.Domain.Validators;

internal sealed class GoodsValidator: AbstractValidator<IReadOnlyCollection<GoodModel>>
{
    public GoodsValidator()
    {
        RuleForEach(x => x)
            .SetValidator(new GoodModelValidator());
    }
}