using AutoBogus;
using Bogus;
using Route256.Week5.Homework.PriceCalculator.Bll.Commands;

namespace Route256.Week5.Homework.PriceCalculator.UnitTests.Fakers;

public static class ClearHistoryCommandFaker
{
    private static readonly object Lock = new();

    private static readonly Faker<ClearHistoryCommand> Faker = new AutoFaker<ClearHistoryCommand>()
        .RuleFor(x => x.UserId, f => f.Random.Long(0L))
        .RuleFor(x => x.CalculationIds,
            f => f.Random.ArrayElements(new long[] { 1, 2, 3, 4, 5 }, 3));

    public static ClearHistoryCommand Generate()
    {
        lock (Lock)
        {
            return Faker.Generate();
        }
    }
    public static ClearHistoryCommand WithUserId(
        this ClearHistoryCommand src, 
        long userId)
    {
        return src with { UserId = userId };
    }
    
    public static ClearHistoryCommand WithCalculationIds(
        this ClearHistoryCommand src, 
        long[] calculationIds)
    {
        return src with { CalculationIds = calculationIds };
    }
}