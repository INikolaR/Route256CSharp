using Route256.Week1.Homework.PriceCalculator.Api.Responses.V1;

namespace Route256.Week1.Homework.PriceCalculator.Api.Responses.V3;

public record GetHistoryResponse(
    CargoResponse Cargo,
    decimal Price,
    decimal Distance);