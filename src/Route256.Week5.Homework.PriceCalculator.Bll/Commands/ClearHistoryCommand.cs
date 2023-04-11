using MediatR;
using Route256.Week5.Homework.PriceCalculator.Bll.Exceptions;
using Route256.Week5.Homework.PriceCalculator.Bll.Models;
using Route256.Week5.Homework.PriceCalculator.Bll.Services.Interfaces;

namespace Route256.Week5.Homework.PriceCalculator.Bll.Commands;

public record ClearHistoryCommand(
        long UserId,
        long[] CalculationIds)
    : IRequest<ClearHistoryResult>;

public class ClearHistoryCommandHandler
    : IRequestHandler<ClearHistoryCommand, ClearHistoryResult>
{
    private readonly ICalculationService _calculationService;

    public ClearHistoryCommandHandler(ICalculationService calculationService)
    {
        _calculationService = calculationService;
    }
    public async Task<ClearHistoryResult> Handle(
        ClearHistoryCommand request,
        CancellationToken cancellationToken)
    {
        var anotherUsersCalculations =
            await _calculationService.CalculationsBelongToAnotherUser(
                new QueryModel(request.UserId, request.CalculationIds),
                cancellationToken);
        if (anotherUsersCalculations.Length != 0)
        {
            throw new OneOrManyCalculationsBelongsToAnotherUserException(anotherUsersCalculations);
        }
        var absentCalculations = 
            await _calculationService.AbsentCalculations(
                new QueryModel(request.UserId, request.CalculationIds),
                cancellationToken);
        if (absentCalculations.Length != 0)
        {
            throw new OneOrManyCalculationsNotFoundException();
        }
        var clearAll = request.CalculationIds.Length == 0;
        var connectedGoodIds = clearAll
            ? await _calculationService.AllConnectedGoodIdsQuery(
                request.UserId,
                cancellationToken)
            : await _calculationService.ConnectedGoodIdsQuery(
                new QueryModel(request.UserId, request.CalculationIds),
                cancellationToken);
        if (clearAll)
        {
            await _calculationService.ClearAllHistory(
                new ClearAllHistoryModel(request.UserId,
                    connectedGoodIds), cancellationToken);
        }
        else
        {
            await _calculationService.ClearHistory(
                new ClearHistoryModel(connectedGoodIds,
                    request.CalculationIds), cancellationToken);
        }
        return new ClearHistoryResult();
    }
}