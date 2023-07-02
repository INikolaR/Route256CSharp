namespace Route256.Week6.Homework.PriceCalculator.BackgroundServices.Messages;

public record GoodMessage(
    long GoodId,
    double Height,
    double Length,
    double Width,
    double Weight);