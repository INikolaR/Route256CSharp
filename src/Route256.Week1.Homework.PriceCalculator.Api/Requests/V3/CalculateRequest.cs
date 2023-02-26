using Route256.Week1.Homework.PriceCalculator.Api.Requests.V2;

namespace Route256.Week1.Homework.PriceCalculator.Api.Requests.V3;

public record CalculateRequest(
    GoodProperties[] Goods,
    int Distance);