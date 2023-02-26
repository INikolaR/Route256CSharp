using Microsoft.AspNetCore.Mvc;
using Route256.Week1.Homework.PriceCalculator.Api.Bll.Models.PriceCalculator;
using Route256.Week1.Homework.PriceCalculator.Api.Bll.Services.Interfaces;
using Route256.Week1.Homework.PriceCalculator.Api.Requests.V3;
using Route256.Week1.Homework.PriceCalculator.Api.Responses.V3;
using CalculateRequest = Route256.Week1.Homework.PriceCalculator.Api.Requests.V3.CalculateRequest;
using CalculateResponse = Route256.Week1.Homework.PriceCalculator.Api.Responses.V3.CalculateResponse;

namespace Route256.Week1.Homework.PriceCalculator.Api.Controllers;

[ApiController]
[Route("/v3/[controller]")]
public class V3DeliveryPriceController : ControllerBase
{
    private readonly IPriceCalculatorService _priceCalculatorService;

    public V3DeliveryPriceController(
        IPriceCalculatorService priceCalculatorService)
    {
        _priceCalculatorService = priceCalculatorService;
    }
    
    /// <summary>
    /// Метод расчета стоимости доставки на основе объема товаров
    /// или веса товара. Окончательная стоимость принимается как наибольшая
    /// </summary>
    /// <returns></returns>
    [HttpPost("calculate")]
    public CalculateResponse Calculate(
        CalculateRequest request)
    {
        var price = _priceCalculatorService.CalculatePrice(
            request.Goods
                .Select(x => new GoodModel(
                    x.Height,
                    x.Length,
                    x.Width,
                    x.Weight))
                .ToArray(),
            request.Distance);
        
        return new CalculateResponse(price);
    }
    
    /// <summary>
    /// Метод получения истории вычисления
    /// </summary>
    /// <returns></returns>
    [HttpPost("get-history")]
    public GetHistoryResponse[] History(GetHistoryRequest request)
    {
        var log = _priceCalculatorService.QueryLog(request.Take);

        return log
            .Select(x => new GetHistoryResponse(
                new CargoResponse(
                    x.Volume,
                    x.Weight),
                x.Price,
                x.Distance))
            .ToArray();
    }
    
}