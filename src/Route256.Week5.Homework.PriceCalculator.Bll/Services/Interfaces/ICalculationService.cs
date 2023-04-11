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
        ClearHistoryModel command,
        CancellationToken token);

    Task<ClearHistoryResult> ClearAllHistory(
        ClearAllHistoryModel model,
        CancellationToken token);

    Task<long[]> CalculationsBelongToAnotherUser(
        QueryModel model,
        CancellationToken token);

    Task<long[]> AbsentCalculations(
        QueryModel model,
        CancellationToken token);

    Task<long[]> AllConnectedGoodIdsQuery(
        long userId,
        CancellationToken token);

    Task<long[]> ConnectedGoodIdsQuery(
        QueryModel model,
        CancellationToken token);
}