namespace Route256.Week5.Homework.PriceCalculator.Bll.Models;

public record QueryModel(
    long UserId,
    long[] CalculationIds);