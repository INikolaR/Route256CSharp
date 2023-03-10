namespace Route256.PriceCalculator.Api.Bll.Models.PriceCalculator;

public record CalculationLogModel(
    decimal Volume,
    decimal Weight,
    decimal Distance,
    decimal Price);