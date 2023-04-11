using AutoBogus;
using Bogus;
using Route256.Week5.Homework.PriceCalculator.Bll.Models;

namespace Route256.Week5.Homework.PriceCalculator.UnitTests.Fakers;

public static class QueryModelFaker
{
    private static readonly object Lock = new();
    
    private static readonly Faker<QueryModel> Faker = new AutoFaker<QueryModel>()
        .RuleFor(x => x.UserId, 
            f => f.Random.Long(0L))
        .RuleFor(x => x.CalculationIds,
            f => f.Random.ArrayElements(new long[] {1, 2, 3, 4, 5}, 3));
    
    public static QueryModel Generate()
    {
        lock (Lock)
        {
            return Faker.Generate();
        }
    }
    
    public static QueryModel WithUserId(
        this QueryModel src, 
        long userId)
    {
        return src with { UserId = userId };
    }
    
    public static QueryModel WithCalculationIds(
        this QueryModel src, 
        long[] calculationIds)
    {
        return src with { CalculationIds = calculationIds };
    }
}