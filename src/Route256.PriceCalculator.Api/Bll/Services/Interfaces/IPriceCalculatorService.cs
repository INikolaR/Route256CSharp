using Route256.PriceCalculator.Api.Bll.Models.PriceCalculator;

namespace Route256.PriceCalculator.Api.Bll.Services.Interfaces;

public interface IPriceCalculatorService
{
    CalculationLogModel[] QueryLog(int take);
    decimal CalculatePrice(IReadOnlyList<GoodModel> goods);
    decimal CalculatePrice(CalculateRequest request);
}