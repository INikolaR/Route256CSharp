namespace Route256.Week1.Homework.PriceCalculator.Api.Bll.Models.Report;

public record ReportModel(
    decimal MaxWeight,
    decimal MaxVolume,
    decimal MaxDistanceForHeaviestGood,
    decimal MaxDistanceForLargestGood,
    decimal WAvgPrice);