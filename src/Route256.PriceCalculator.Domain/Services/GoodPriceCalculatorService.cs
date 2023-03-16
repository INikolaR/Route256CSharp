using System;
using Route256.PriceCalculator.Domain.Entities;
using Route256.PriceCalculator.Domain.Models.PriceCalculator;
using Route256.PriceCalculator.Domain.Separated;
using Route256.PriceCalculator.Domain.Services.Interfaces;

namespace Route256.PriceCalculator.Domain.Services;

internal sealed class GoodPriceCalculatorService : IGoodPriceCalculatorService
{
    private readonly IGoodsRepository _repository;
    private readonly IPriceCalculatorService _service;

    public GoodPriceCalculatorService(
        IGoodsRepository repository,
        IPriceCalculatorService service)
    {
        _repository = repository;
        _service = service;
    }
    
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
    
    private static GoodModel ToModel(GoodEntity x) 
        => new(x.Height, x.Length, x.Width, x.Weight);
}