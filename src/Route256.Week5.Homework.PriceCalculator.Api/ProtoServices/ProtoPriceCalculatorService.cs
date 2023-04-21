using Grpc.Core;
using MediatR;
using Route256.Week5.Homework.PriceCalculator.Api.Responses.V1;
using Route256.Week5.Homework.PriceCalculator.Bll.Commands;
using Route256.Week5.Homework.PriceCalculator.Bll.Models;
using Route256.Week5.Homework.PriceCalculator.Bll.Queries;

namespace Route256.Week5.Homework.PriceCalculator.Api.ProtoServices;

public class ProtoPriceCalculatorService : Calculator.CalculatorBase
{
    private readonly IMediator _mediator;
    
    public ProtoPriceCalculatorService(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<ProtoCalculationResponse> ProtoCalculatePrice(
        ProtoCalculationRequest request, ServerCallContext context)
    {
        var command = new CalculateDeliveryPriceCommand(
            request.UserId,
            request.Goods
                .Select(x => new GoodModel(
                    x.Height,
                    x.Length,
                    x.Width,
                    x.Weight))
                .ToArray());
        var result = await _mediator.Send(command);

        return new ProtoCalculationResponse{
            CalculationId = result.CalculationId,
            Price = (double)result.Price};
    }

    public override async Task<ProtoClearHistoryResponse> ProtoClearHistory(ProtoClearHistoryRequest request, ServerCallContext context)
    {
        var query = new ClearHistoryCommand(
            request.UserId,
            request.CalculationIds.ToArray()
        );

        await _mediator.Send(query);

        return new ProtoClearHistoryResponse();
    }

    public override async Task ProtoGetHistory(ProtoGetHistoryRequest request, IServerStreamWriter<ProtoGetHistoryResponse> responseStream, ServerCallContext context)
    {
        var query = new GetCalculationHistoryQuery(
            request.UserId,
           int.MaxValue,
            0);
        var result = await _mediator.Send(query);

        var items =  result.Items
            .Select(x => new GetHistoryResponse(
                new GetHistoryResponse.CargoResponse(
                    x.Volume,
                    x.Weight,
                    x.GoodIds),
                x.Price))
            .ToArray();
        foreach (var item in items)
        {
            await responseStream.WriteAsync(new ProtoGetHistoryResponse
            {
                Cargo = new ProtoCargoResponse
                {
                    Volume = item.Cargo.Volume,
                    Weight = item.Cargo.Weight,
                    GoodIds = { item.Cargo.GoodIds }
                },
                Price = (double)item.Price
            });
        }
    }

    public override async Task ProtoBidirectionalStreamingCalculatePrice(IAsyncStreamReader<ProtoCalculationRequest> requestStream, IServerStreamWriter<ProtoCalculationResponse> responseStream,
        ServerCallContext context)
    {
        await foreach (var request in requestStream.ReadAllAsync())
        {
            var command = new CalculateDeliveryPriceCommand(
                        request.UserId,
                        request.Goods
                            .Select(x => new GoodModel(
                                x.Height,
                                x.Length,
                                x.Width,
                                x.Weight))
                            .ToArray());
                    var result = await _mediator.Send(command);
            
                    await responseStream.WriteAsync(new ProtoCalculationResponse{
                        CalculationId = result.CalculationId,
                        Price = (double)result.Price});
        }
    }
}