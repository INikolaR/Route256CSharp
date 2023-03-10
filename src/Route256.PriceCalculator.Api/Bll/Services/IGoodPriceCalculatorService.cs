namespace Route256.PriceCalculator.Api.Bll.Services;

public interface IGoodPriceCalculatorService
{
    decimal сalculatePrice(
        int good_Id, 
        decimal dstns);
}