using MediatR;
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
        await _calculationService.ClearHistory(request, cancellationToken);
        return new ClearHistoryResult();
    }
}