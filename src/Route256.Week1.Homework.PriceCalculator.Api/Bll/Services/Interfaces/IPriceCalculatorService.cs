using Route256.Week1.Homework.PriceCalculator.Api.Bll.Models.PriceCalculator;
using Route256.Week1.Homework.PriceCalculator.Api.Bll.Models.Report;

namespace Route256.Week1.Homework.PriceCalculator.Api.Bll.Services.Interfaces;

public interface IPriceCalculatorService
{
    private const int DistanceToPriceRatio = 1000;
    decimal CalculatePrice(IReadOnlyList<GoodModel> goods, int distance = DistanceToPriceRatio);

    CalculationLogModel[] QueryLog(int take);
    void DeleteHistory();

    ReportModel GetReport();
}