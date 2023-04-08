namespace Route256.Week5.Homework.PriceCalculator.Dal.Models;

public record ClearHistoryCommandModel(
    long UserId,
    long[] CalculationIds
    );