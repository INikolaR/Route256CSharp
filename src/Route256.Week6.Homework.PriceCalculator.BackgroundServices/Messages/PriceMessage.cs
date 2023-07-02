namespace Route256.Week6.Homework.PriceCalculator.BackgroundServices.Messages;

public record PriceMessage(
    long GoodId,
    decimal Price);