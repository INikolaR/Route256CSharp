namespace Route256.PriceCalculator.Api.Bll.Models.PriceCalculator;

public sealed record CalculateRequest(GoodModel[] Goods, decimal Distance);