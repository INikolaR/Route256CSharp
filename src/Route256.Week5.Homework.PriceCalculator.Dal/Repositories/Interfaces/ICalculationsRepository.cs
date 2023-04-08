using Route256.Week5.Homework.PriceCalculator.Dal.Entities;
using Route256.Week5.Homework.PriceCalculator.Dal.Models;

namespace Route256.Week5.Homework.PriceCalculator.Dal.Repositories.Interfaces;

public interface ICalculationRepository : IDbRepository
{
    Task<long[]> Add(
        CalculationEntityV1[] entityV1, 
        CancellationToken token);

    Task<CalculationEntityV1[]> Query(
        CalculationHistoryQueryModel query,
        CancellationToken token);

    Task<long[]> ConnectedGoodIdsQuery(
        ClearHistoryCommandModel command,
        CancellationToken token);

    Task ClearHistory(
        ClearHistoryCommandModel command,
        CancellationToken token);

    Task<long[]> AllConnectedGoodIdsQuery(
        long userId,
        CancellationToken token);

    Task ClearAllHistory(
        long userId,
        CancellationToken token);

    Task<long[]> CalculationsBelongToAnotherUser(
        ClearHistoryCommandModel command,
        CancellationToken token);

    Task<long[]> AbsentCalculations(
        ClearHistoryCommandModel command,
        CancellationToken token);
}