using Route256.PriceCalculator.Domain.Entities;

namespace Route256.PriceCalculator.Domain.Separated;

public interface IGoodsService
{
    IEnumerable<GoodEntity> GetGoods();
}