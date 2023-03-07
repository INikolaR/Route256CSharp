using Route256.Week1.Homework.PriceCalculator.Api.Requests.V2;

namespace Route256.Week1.Homework.PriceCalculator.Api.Requests.V3;

/// <summary>
/// Товары, чью цену транспортировки нужно расчитать, а также расстояние перевозки.
/// </summary>
public record CalculateRequest(
    int Distance
);