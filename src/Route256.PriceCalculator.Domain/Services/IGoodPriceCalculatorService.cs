namespace Route256.PriceCalculator.Domain.Services;

public interface IGoodPriceCalculatorService
{
    decimal сalculatePrice(
        int good_Id, 
        decimal dstns);
}