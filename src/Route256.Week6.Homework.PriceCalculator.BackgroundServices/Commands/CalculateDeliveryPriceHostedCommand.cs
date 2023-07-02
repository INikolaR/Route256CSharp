using MediatR;
using Route256.Week6.Homework.PriceCalculator.BackgroundServices.Messages;

namespace Route256.Week6.Homework.PriceCalculator.BackgroundServices.Commands;

public record CalculateDeliveryPriceHostedCommand(
        GoodMessage Message)
    : IRequest<PriceMessage>;

public class CalculateDeliveryPriceHostedCommandHandler 
    : IRequestHandler<CalculateDeliveryPriceHostedCommand, PriceMessage>
{
    public const decimal VolumeToPriceRatio = 3.27m;
    public const decimal WeightToPriceRatio = 1.34m;
    
    public Task<PriceMessage> Handle(
        CalculateDeliveryPriceHostedCommand request, 
        CancellationToken cancellationToken)
    {
        var volumePrice = CalculatePriceByVolume(request.Message);
        var weightPrice = CalculatePriceByWeight(request.Message);
        var resultPrice = Math.Max(volumePrice, weightPrice);
        
        var response = new PriceMessage(
            request.Message.GoodId,
            resultPrice);
        return Task.FromResult(response);
    }

    private decimal CalculatePriceByVolume(GoodMessage good)
    {
        return (decimal)(good.Height * good.Width * good.Height * (double)VolumeToPriceRatio);
    }
    
    private decimal CalculatePriceByWeight(GoodMessage good)
    {
        return (decimal)(good.Weight * (double)WeightToPriceRatio);
    }
    
}