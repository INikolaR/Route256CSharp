using Route256.Week1.Homework.PriceCalculator.Api.Bll.Models.PriceCalculator;
using Route256.Week1.Homework.PriceCalculator.Api.Bll.Models.Report;
using Route256.Week1.Homework.PriceCalculator.Api.Bll.Services.Interfaces;
using Route256.Week1.Homework.PriceCalculator.Api.Dal.Entities;
using Route256.Week1.Homework.PriceCalculator.Api.Dal.Repositories.Interfaces;

namespace Route256.Week1.Homework.PriceCalculator.Api.Bll.Services;

public class PriceCalculatorService : IPriceCalculatorService
{
    private const decimal volumeToPriceRatio = 3.27m;
    private const decimal weightToPriceRatio = 1.34m;
    
    private readonly IStorageRepository _storageRepository;
    
    public PriceCalculatorService(
        IStorageRepository storageRepository)
    {
        _storageRepository = storageRepository;
    }
    
    public decimal CalculatePrice(IReadOnlyList<GoodModel> goods, decimal distance = 1000)
    {
        if (distance < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(distance));
        }
        if (!goods.Any())
        {
            throw new ArgumentOutOfRangeException(nameof(goods));
        }

        var volumePrice = CalculatePriceByVolume(goods, out var volume);
        var weightPrice = CalculatePriceByWeight(goods, out var weight);

        var resultPrice = Math.Max(volumePrice, weightPrice) * (distance / 1000);
        var quantity = goods.Count;

        _storageRepository.Save(new StorageEntity(
            DateTime.UtcNow,
            volume / 1e6m,
            weight * 1000,
            resultPrice,
            quantity,
            distance));
        
        return resultPrice;
    }

    private decimal CalculatePriceByVolume(
        IReadOnlyList<GoodModel> goods,
        out decimal volume)
    {
        volume = goods
            .Select(x => x.Height * x.Width * x.Length * 1e6m)
            .Sum();

        return volume * volumeToPriceRatio;
    }
    
    private decimal CalculatePriceByWeight(
        IReadOnlyList<GoodModel> goods,
        out decimal weight)
    {
        weight = goods
            .Select(x => x.Weight / 1000)
            .Sum();

        return weight * weightToPriceRatio;
    }

    public CalculationLogModel[] QueryLog(int take)
    {
        if (take == 0)
        {
            return Array.Empty<CalculationLogModel>();
        }
        
        var log = _storageRepository.Query()
            .OrderByDescending(x => x.At)
            .Take(take)
            .ToArray();

        return log
            .Select(x => new CalculationLogModel(
                x.Volume, 
                x.Weight,
                x.Price,
                x.Distance))
            .ToArray();
    }

    public void DeleteHistory()
    {
        _storageRepository.Clear();
    }

    public ReportModel GetReport()
    {
        var report = _storageRepository.Query();
        if (!report.Any())
        {
            return new ReportModel(
                0,
                0,
                0,
                0,
                0);
        }

        var maxWeight = report.Max(x => x.Weight);
        var maxVolume = report.Max(x => x.Volume);
        return new ReportModel(
            maxWeight,
            maxVolume,
            report.Where(x => x.Weight == maxWeight)
                                      .Max(x => x.Distance),
            report.Where(x => x.Volume == maxVolume)
                                      .Max(x => x.Distance),
            report.Sum(x => x.Price * x.Quantity)
                        / report.Sum(x => x.Quantity));

    }
}