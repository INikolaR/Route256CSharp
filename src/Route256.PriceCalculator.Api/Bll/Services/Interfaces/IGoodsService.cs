using Route256.PriceCalculator.Api.Dal.Entities;

namespace Route256.PriceCalculator.Api.Bll.Services.Interfaces;

public interface IGoodsService
{
    IEnumerable<GoodEntity> GetGoods();
}