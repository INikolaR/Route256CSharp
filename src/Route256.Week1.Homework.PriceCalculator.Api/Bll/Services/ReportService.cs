using Route256.Week1.Homework.PriceCalculator.Api.Bll.Models.Report;
using Route256.Week1.Homework.PriceCalculator.Api.Bll.Services.Interfaces;
using Route256.Week1.Homework.PriceCalculator.Api.Dal.Repositories;
using Route256.Week1.Homework.PriceCalculator.Api.Dal.Repositories.Interfaces;

namespace Route256.Week1.Homework.PriceCalculator.Api.Bll.Services;

public class ReportService : IReportService
{
    private const decimal WeightToTonnesRatio = 1m;
    private const decimal VolumeToCentimeters3Ratio = 1e3m;
    private readonly IStorageRepository _storageRepository;
    public ReportService(
        IStorageRepository storageRepository)
    {
        _storageRepository = storageRepository;
    }

    public ReportModel GetReport()
    {
        var query = _storageRepository.Query();
        if (!query.Any())
        {
            return new ReportModel(
                0,
                0,
                0,
                0,
                0);
        }
        
        var maxWeight = query.Max(x => x.Weight);
        var maxVolume = query.Max(x => x.Volume);
        return new ReportModel(
            maxWeight * WeightToTonnesRatio,
            maxVolume * VolumeToCentimeters3Ratio,
            query.Where(x => x.Weight == maxWeight)
                .Max(x => x.Distance),
            query.Where(x => x.Volume == maxVolume)
                .Max(x => x.Distance),
            // Для подсчёта средневзвешенной по количеству товаров стоимости
            // нужно разделить сумму произведений количества товара с его стоимостью
            // на сумму количеств товаров (формула есть на
            // https://ru.wikipedia.org/wiki/Среднее_арифметическое_взвешенное,
            // в качестве усредняемого берется цена, в качестве веса - Quantity).
            query.Sum(x => x.Price * x.Quantity)
            / query.Sum(x => x.Quantity));

    }
}