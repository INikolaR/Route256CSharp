using Microsoft.AspNetCore.Mvc;
using Route256.Week1.Homework.PriceCalculator.Api.Bll.Models.PriceCalculator;
using Route256.Week1.Homework.PriceCalculator.Api.Bll.Services;
using Route256.Week1.Homework.PriceCalculator.Api.Bll.Services.Interfaces;
using Route256.Week1.Homework.PriceCalculator.Api.Dal.Entities;
using Route256.Week1.Homework.PriceCalculator.Api.Dal.Repositories.Interfaces;
using Route256.Week1.Homework.PriceCalculator.Api.Requests.V3;
using Route256.Week1.Homework.PriceCalculator.Api.Responses.V3;

namespace Route256.Week1.Homework.PriceCalculator.Api.Controllers;

[ApiController]
public class V3DeliveryPriceController
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<V3DeliveryPriceController> _logger;
    private readonly IGoodsRepository _repository;

    public V3DeliveryPriceController(
        IHttpContextAccessor httpContextAccessor,
        ILogger<V3DeliveryPriceController> logger,
        IGoodsRepository repository)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _repository = repository;
    }


    /// <summary>
    /// Метод для вычисления полной стоимости товара (стоимость доставки + стоимость самого товара).
    /// </summary>
    /// <param name="priceCalculatorDistanceService"></param>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("calculateFullPrice/{id}")]
    public CalculateResponse CalculateFullPrice(
        CalculateRequest request,
        [FromServices] IPriceCalculatorDistanceService priceCalculatorDistanceService, 
        int id)
    {
        _logger.LogInformation(_httpContextAccessor.HttpContext.Request.Path);
        
        var good = _repository.Get(id);
        var model = new GoodModel(
            good.Height,
            good.Length,
            good.Width,
            good.Weight);
        
        // Суммируем цену за доставку товара с ценой самого товара.
        var price = priceCalculatorDistanceService.CalculatePrice(new []{ model }, request.Distance) + good.Price;
        return new CalculateResponse(price);
    }
}