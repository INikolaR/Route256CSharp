using Route256.PriceCalculator.Api.Bll.Models.PriceCalculator;
using Route256.PriceCalculator.Api.Bll.Services.Interfaces;
using Route256.PriceCalculator.Api.Dal.Entities;
using Route256.PriceCalculator.Api.Dal.Repositories.Interfaces;

namespace Route256.PriceCalculator.Api.Bll.Services;

public sealed class GoodPriceCalculatorService : IGoodPriceCalculatorService
{
    private readonly IGoodsRepository Repository;
    private readonly IPriceCalculatorService Service;

    public GoodPriceCalculatorService(
        IGoodsRepository Repository,
        IPriceCalculatorService Service)
    {
        this.Repository = Repository;
        this.Service = Service;
    }
    
    public decimal сalculatePrice(
        int good_Id, 
        decimal dstns)
    {
        if (good_Id == default)
            throw new ArgumentException($"{nameof(good_Id)} is default");
        
        if (dstns == default)
            throw new ArgumentException($"{nameof(dstns)} is default");
        
        var g = Repository.Get(good_Id);
        var m = to_Model(g);
        
        return Service.CalculatePrice(new []{ m }) * dstns;
    }
    
    private static GoodModel to_Model(GoodEntity x) 
        => new(x.Height, x.Length, x.Width, x.Weight);
}