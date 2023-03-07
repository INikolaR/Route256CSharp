using Route256.Week1.Homework.PriceCalculator.Api.Bll.Models.PriceCalculator;

namespace Route256.Week1.Homework.PriceCalculator.Api.Bll.Services;
/// <summary>
/// Интерфейс для нового сервиса.
/// </summary>
public interface IPriceCalculatorDistanceService
{
    decimal CalculatePrice(IReadOnlyList<GoodModel> goods, int distance = 1000);
    CalculationLogModel[] QueryLog(int take);
}