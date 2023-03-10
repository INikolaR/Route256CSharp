namespace Route256.PriceCalculator.Api.Dal.Entities;

public record StorageEntity(
    DateTime At,
    decimal Volume,
    decimal Weight,
    decimal Distance,
    decimal Price);
