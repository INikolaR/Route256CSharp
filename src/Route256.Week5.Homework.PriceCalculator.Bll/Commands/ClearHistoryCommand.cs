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
            await _calculationService.CalculationsBelongToAnotherUser(request, cancellationToken);
        if (anotherUsersCalculations.Length != 0)
        {
            throw new OneOrManyCalculationsBelongsToAnotherUserException(anotherUsersCalculations);
        }
        var absentCalculations = 
            await _calculationService.AbsentCalculations(request, cancellationToken);
        if (absentCalculations.Length != 0)
        {
            throw new OneOrManyCalculationsNotFoundException();
        }
        await _calculationService.ClearHistory(request, cancellationToken);
        return new ClearHistoryResult();
    }
}