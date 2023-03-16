using FluentValidation;
using System.Collections.Generic;
using Route256.PriceCalculator.Domain.Models.PriceCalculator;

namespace Route256.PriceCalculator.Domain.Validators;

internal sealed class GoodModelValidator: AbstractValidator<GoodModel>
{
    public GoodModelValidator()
    {
        RuleFor(x => x.Weight)
            .GreaterThan(0)
            .LessThan(Int32.MaxValue);
        
        RuleFor(x => x.Height)
            .GreaterThan(0)
            .LessThan(Int32.MaxValue);
        
        RuleFor(x => x.Length)
            .GreaterThan(0)
            .LessThan(Int32.MaxValue);

        RuleFor(x => x.Width)
            .GreaterThan(0)
            .LessThan(Int32.MaxValue);
    }
}