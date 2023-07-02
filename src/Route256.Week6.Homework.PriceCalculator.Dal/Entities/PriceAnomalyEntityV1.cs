namespace Route256.Week5.Workshop.PriceCalculator.Dal.Entities;

public record PriceAnomalyEntityV1
{
    public long Id { get; init; }
    
    public long GoodId { get; init; }

    public decimal Price { get; init; }
}