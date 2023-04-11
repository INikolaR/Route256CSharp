using Route256.Week5.Homework.PriceCalculator.Bll.Commands;
using Route256.Week5.Homework.PriceCalculator.Bll.Models;
using Route256.Week5.Homework.PriceCalculator.Bll.Services.Interfaces;
using Route256.Week5.Homework.PriceCalculator.Dal.Entities;
using Route256.Week5.Homework.PriceCalculator.Dal.Models;
using Route256.Week5.Homework.PriceCalculator.Dal.Repositories.Interfaces;

namespace Route256.Week5.Homework.PriceCalculator.Bll.Services;

public class CalculationService : ICalculationService
{
    public const decimal VolumeToPriceRatio = 3.27m;
    public const decimal WeightToPriceRatio = 1.34m;

    private readonly ICalculationRepository _calculationRepository;
    private readonly IGoodsRepository _goodsRepository;

    public CalculationService(
        ICalculationRepository calculationRepository,
        IGoodsRepository goodsRepository)
    {
        _calculationRepository = calculationRepository;
        _goodsRepository = goodsRepository;
    }
    
    public async Task<long> SaveCalculation(
        SaveCalculationModel data,
        CancellationToken cancellationToken)
    {
        var goods = data.Goods
            .Select(x => new GoodEntityV1
            {
                UserId = data.UserId,
                Height = x.Height,
                Weight = x.Weight,
                Length = x.Length,
                Width = x.Width
            })
            .ToArray();
        
        var calculation = new CalculationEntityV1
        {
            UserId = data.UserId,
            TotalVolume = data.TotalVolume,
            TotalWeight = data.TotalWeight,
            Price = data.Price,
            At = DateTimeOffset.UtcNow
        };
        
        using var transaction = _calculationRepository.CreateTransactionScope();
        var goodIds = await _goodsRepository.Add(goods, cancellationToken);

        calculation = calculation with {GoodIds = goodIds};
        var calculationIds = await _calculationRepository.Add(new[] {calculation}, cancellationToken);
        transaction.Complete();

        return calculationIds.Single();
    }
    
    public decimal CalculatePriceByVolume(
        GoodModel[] goods,
        out double volume)
    {
        volume = goods
            .Sum(x => x.Length * x.Width * x.Height);

        return (decimal)volume * VolumeToPriceRatio;
    }
    
    public decimal CalculatePriceByWeight(
        GoodModel[] goods,
        out double weight)
    {
        weight = goods
            .Sum(x => x.Weight);

        return (decimal)weight * WeightToPriceRatio;
    }

    public async Task<QueryCalculationModel[]> QueryCalculations(
        QueryCalculationFilter query,
        CancellationToken token)
    {
        var result = await _calculationRepository.Query(new CalculationHistoryQueryModel(
                query.UserId,
                query.Limit,
                query.Offset),
            token);

        return result
            .Select(x => new QueryCalculationModel(
                x.Id,
                x.UserId,
                x.TotalVolume,
                x.TotalWeight,
                x.Price,
                x.GoodIds))
            .ToArray();
    }
    
    public async Task<ClearHistoryResult> ClearHistory(
        ClearHistoryModel model,
        CancellationToken token)
    {
        using var transaction = _calculationRepository.CreateTransactionScope();
        await _calculationRepository.ClearHistory(model.CalculationIds,
                token);
        await _goodsRepository.ClearHistory(model.ConnectedGoodIds, token);
        transaction.Complete();
        return new ClearHistoryResult();
    }
    
    public async Task<ClearHistoryResult> ClearAllHistory(
        ClearAllHistoryModel model,
        CancellationToken token)
    {
        using var transaction = _calculationRepository.CreateTransactionScope();
        await _calculationRepository.ClearAllHistory(model.UserId, token);
        await _goodsRepository.ClearHistory(model.ConnectedGoodIds, token);
        transaction.Complete();
        return new ClearHistoryResult();
    }

    public async Task<long[]> CalculationsBelongToAnotherUser(
        QueryModel model,
        CancellationToken token)
    {
        var result =  await _calculationRepository.CalculationsBelongToAnotherUser(
            new ClearHistoryCommandModel(model.UserId, model.CalculationIds),
            token);
        return result;
    }
    
    public async Task<long[]> AbsentCalculations(
        QueryModel model,
        CancellationToken token)
    {
        var result =  await _calculationRepository.AbsentCalculations(
            new ClearHistoryCommandModel(model.UserId, model.CalculationIds),
            token);
        return result;
    }
    
    public async Task<long[]> AllConnectedGoodIdsQuery(
        long userId,
        CancellationToken token)
    {
        var result =  await _calculationRepository.AllConnectedGoodIdsQuery(
            userId,
            token);
        return result;
    }
    
    public async Task<long[]> ConnectedGoodIdsQuery(
        QueryModel model,
        CancellationToken token)
    {
        var result =  await _calculationRepository.ConnectedGoodIdsQuery(
            new ClearHistoryCommandModel(model.UserId, model.CalculationIds),
            token);
        return result;
    }
}