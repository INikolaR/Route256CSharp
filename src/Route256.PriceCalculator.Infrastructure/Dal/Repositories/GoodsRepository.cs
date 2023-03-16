using System.Collections.Generic;
using System.Linq;
using Route256.PriceCalculator.Domain.Entities;
using Route256.PriceCalculator.Domain.Separated;
using Route256.PriceCalculator.Infrastructure.Exceptions;

namespace Route256.PriceCalculator.Infrastructure.Dal.Repositories;

public sealed class GoodsRepository : IGoodsRepository
{
    private readonly Dictionary<int, GoodEntity> _store = new Dictionary<int, GoodEntity>();

    public void AddOrUpdate(GoodEntity entity)
    {
        if (_store.ContainsKey(entity.Id))
            _store.Remove(entity.Id);
        
        _store.Add(entity.Id, entity);
    }

    public ICollection<GoodEntity> GetAll()
    {
        return _store.Select(x => x.Value).ToArray();
    }

    public GoodEntity Get(int id)
    {
        try
        {
            return _store[id];
        }
        catch (KeyNotFoundException e)
        {
            throw new EntityNotFoundException("Not found", e);
        }
    }
}