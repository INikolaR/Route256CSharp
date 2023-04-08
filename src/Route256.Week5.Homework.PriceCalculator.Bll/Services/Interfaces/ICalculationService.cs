using Route256.Week5.Homework.PriceCalculator.Bll.Commands;
using Route256.Week5.Homework.PriceCalculator.Bll.Models;

namespace Route256.Week5.Homework.PriceCalculator.Bll.Services.Interfaces;

public interface ICalculationService
{
    Task<long> SaveCalculation(
        SaveCalculationModel saveCalculation,
        CancellationToken cancellationToken);

    decimal CalculatePriceByVolume(
        GoodModel[] goods,
        out double volume);

    public decimal CalculatePriceByWeight(
        GoodModel[] goods,
        out double weight);

    Task<QueryCalculationModel[]> QueryCalculations(
        QueryCalculationFilter query,
        CancellationToken token);

    Task<ClearHistoryResult> ClearHistory(
        ClearHistoryCommand command,
        CancellationToken token);

    Task<long[]> CalculationsBelongToAnotherUser(
        ClearHistoryCommand command,
        CancellationToken token);

    Task<long[]> AbsentCalculations(
        ClearHistoryCommand command,
        CancellationToken token);
}