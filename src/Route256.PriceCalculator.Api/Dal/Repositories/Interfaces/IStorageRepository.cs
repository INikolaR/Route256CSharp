using Route256.PriceCalculator.Api.Dal.Entities;

namespace Route256.PriceCalculator.Api.Dal.Repositories.Interfaces;

public interface IStorageRepository
{
    void Save(StorageEntity entity);

    IReadOnlyList<StorageEntity> Query();
}