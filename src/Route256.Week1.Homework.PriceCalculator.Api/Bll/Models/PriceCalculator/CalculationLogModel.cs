namespace Route256.Week1.Homework.PriceCalculator.Api.Bll.Models.PriceCalculator;

public record CalculationLogModel(
    decimal Volume, // см3
    decimal Weight, // т
    decimal Price, // р
    decimal Distance); // км