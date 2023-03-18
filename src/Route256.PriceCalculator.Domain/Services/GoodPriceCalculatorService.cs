using System;
using Route256.PriceCalculator.Domain.Entities;
using Route256.PriceCalculator.Domain.Models.PriceCalculator;
using Route256.PriceCalculator.Domain.Separated;
using Route256.PriceCalculator.Domain.Services.Interfaces;

namespace Route256.PriceCalculator.Domain.Services;
/// <summary>
/// Class to calculate price with distance using
/// IPriceCalculatorService object.
/// </summary>
internal sealed class GoodPriceCalculatorService : IGoodPriceCalculatorService
{
    private readonly IGoodsRepository _repository;
    private readonly IPriceCalculatorService _service;
    
    /// <summary>
    /// Makes GoodPriceCalculatorService object using
    /// IGoodRepository and IPriceCalculatorService.
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="service"></param>
    public GoodPriceCalculatorService(
        IGoodsRepository repository,
        IPriceCalculatorService service)
    {
        _repository = repository;
        _service = service;
    }
    /// <summary>
    /// Method to calculate price according to distance.
    /// Multiplies the result of IPriceCalculatorService.CalculatePrice
    /// and distance.
    /// </summary>
    /// <param name="goodId"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public decimal CalculatePrice(
        int goodId, 
        decimal distance)
    {
        if (goodId == default)
            throw new ArgumentException($"{nameof(goodId)} is default");
        
        if (distance == default)
            throw new ArgumentException($"{nameof(distance)} is default");
        
        var goodEntity = _repository.Get(goodId);
        var model = ToModel(goodEntity);
        
        return _service.CalculatePrice(new []{ model }) * distance;
    }
    /// <summary>
    /// Converts GoodEntity to GoodModel.
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    private static GoodModel ToModel(GoodEntity x) 
        => new(x.Height, x.Length, x.Width, x.Weight);
}