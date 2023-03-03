using Route256.Week1.Homework.PriceCalculator.Api.Bll.Models.PriceCalculator;
using Route256.Week1.Homework.PriceCalculator.Api.Bll.Models.Report;
using Route256.Week1.Homework.PriceCalculator.Api.Bll.Services.Interfaces;
using Route256.Week1.Homework.PriceCalculator.Api.Dal.Entities;
using Route256.Week1.Homework.PriceCalculator.Api.Dal.Repositories.Interfaces;

namespace Route256.Week1.Homework.PriceCalculator.Api.Bll.Services;

public class PriceCalculatorService : IPriceCalculatorService
{
    private const decimal VolumeToPriceRatio = 3.27m;
    private const decimal WeightToPriceRatio = 1.34m;
    private const int DistanceToKilometersRatio = 1000;
    private const decimal WeightToTonnesRatio = 1m;
    private const decimal VolumeToCentimeters3Ratio = 1e3m;
    
    private readonly IStorageRepository _storageRepository;
    
    public PriceCalculatorService(
        IStorageRepository storageRepository)
    {
        _storageRepository = storageRepository;
    }
    
    public decimal CalculatePrice(IReadOnlyList<GoodModel> goods, int distance = DistanceToKilometersRatio)
    {
        if (distance < 1) // We must block all distances less than 1.
        {
            throw new ArgumentOutOfRangeException(nameof(distance), "Distance must not be less than 1");
        }
        if (!goods.Any())
        {
            throw new ArgumentOutOfRangeException(nameof(goods));
        }

        var volumePrice = CalculatePriceByVolume(goods, out var volume);
        var weightPrice = CalculatePriceByWeight(goods, out var weight);

        var resultPrice = CalculateResultPrice(volumePrice, weightPrice, distance);
        var quantity = goods.Count;

        _storageRepository.Save(new StorageEntity(
            DateTime.UtcNow,
            volume,
            weight,
            resultPrice,
            quantity,
            distance));
        
        return resultPrice;
    }

    private decimal CalculateResultPrice(decimal volumePrice, decimal weightPrice, decimal distance)
    {
        return Math.Max(volumePrice, weightPrice) * ((decimal)distance / DistanceToKilometersRatio);
    }

    private decimal CalculatePriceByVolume(
        IReadOnlyList<GoodModel> goods,
        out decimal volume)
    {
        volume = goods
            .Select(x => x.Height * x.Width * x.Length / VolumeToCentimeters3Ratio)
            .Sum();

        return volume * VolumeToPriceRatio;
    }
    
    private decimal CalculatePriceByWeight(
        IReadOnlyList<GoodModel> goods,
        out decimal weight)
    {
        weight = goods
            .Select(x => x.Weight / WeightToTonnesRatio)
            .Sum();

        return weight * WeightToPriceRatio;
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
                x.Volume * VolumeToCentimeters3Ratio, 
                x.Weight * WeightToTonnesRatio,
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

        foreach (var item in report)
        {
            Console.WriteLine($"{item.Weight} {item.Volume}");
        }
        var maxWeight = report.Max(x => x.Weight);
        var maxVolume = report.Max(x => x.Volume);
        return new ReportModel(
            maxWeight * WeightToTonnesRatio,
            maxVolume * VolumeToCentimeters3Ratio,
            report.Where(x => x.Weight == maxWeight)
                                      .Max(x => x.Distance),
            report.Where(x => x.Volume == maxVolume)
                                      .Max(x => x.Distance),
            // Для подсчёта средневзвешенной по количеству товаров стоимости
            // нужно разделить сумму произведений количества товара с его стоимостью
            // на сумму количеств товаров (формула есть на
            // https://ru.wikipedia.org/wiki/Среднее_арифметическое_взвешенное,
            // в качестве усредняемого берется цена, в качестве веса - Quantity).
            report.Sum(x => x.Price * x.Quantity)
                        / report.Sum(x => x.Quantity));

    }
}