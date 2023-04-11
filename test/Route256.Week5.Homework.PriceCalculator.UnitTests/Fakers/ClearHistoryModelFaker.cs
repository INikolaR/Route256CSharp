using AutoBogus;
using Bogus;
using Route256.Week5.Homework.PriceCalculator.Bll.Commands;
using Route256.Week5.Homework.PriceCalculator.Bll.Models;
using Route256.Week5.Homework.PriceCalculator.Dal.Models;

namespace Route256.Week5.Homework.PriceCalculator.UnitTests.Fakers;

public static class ClearHistoryModelFaker
{
    private static readonly object Lock = new();
    
    private static readonly Faker<ClearHistoryModel> Faker = new AutoFaker<ClearHistoryModel>()
        .RuleFor(x => x.ConnectedGoodIds, 
            f => f.Random.ArrayElements(new long[] {1, 2, 3, 4, 5}, 3))
        .RuleFor(x => x.CalculationIds,
            f => f.Random.ArrayElements(new long[] {1, 2, 3, 4, 5}, 3));
    
    public static ClearHistoryModel Generate()
    {
        lock (Lock)
        {
            return Faker.Generate();
        }
    }
    
    public static ClearHistoryModel WithConnectedGoodIds(
        this ClearHistoryModel src, 
        long[] connectedGoodIds)
    {
        return src with { ConnectedGoodIds = connectedGoodIds };
    }
    
    public static ClearHistoryModel WithCalculationIds(
        this ClearHistoryModel src, 
        long[] calculationIds)
    {
        return src with { CalculationIds = calculationIds };
    }
}