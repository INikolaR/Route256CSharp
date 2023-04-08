using System.Net;

namespace Route256.Week5.Homework.PriceCalculator.Api.ActionFilters;

public class ErrorResponse
{
    public long[] WrongCalculationIds { get; }

    public ErrorResponse(long[] wrongCalculationIds)
    {
        WrongCalculationIds = (long[])wrongCalculationIds.Clone();
    }
}