using Route256.Week1.Homework.PriceCalculator.Api.Bll.Models.PriceCalculator;
using Route256.Week1.Homework.PriceCalculator.Api.Bll.Models.Report;

namespace Route256.Week1.Homework.PriceCalculator.Api.Bll.Services.Interfaces;

public interface IPriceCalculatorService
{
    decimal CalculatePrice(IReadOnlyList<GoodModel> goods, decimal distance = 1000);

    CalculationLogModel[] QueryLog(int take);
    void DeleteHistory();

    ReportModel GetReport();
}