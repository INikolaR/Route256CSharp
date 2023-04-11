namespace Route256.Week5.Homework.PriceCalculator.Bll.Models;

public record ClearHistoryModel(
    long[] ConnectedGoodIds,
    long[] CalculationIds);