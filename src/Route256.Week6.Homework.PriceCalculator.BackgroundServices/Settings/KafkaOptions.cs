namespace Route256.Week6.Homework.PriceCalculator.BackgroundServices.Settings;

public record KafkaOptions
{
    public string Broker { get; init; } = string.Empty;
    public string InputTopic { get; init; } = string.Empty;
    public string OutputTopic { get; init; } = string.Empty;
    public string DlqTopic { get; init; } = string.Empty;
    
}
