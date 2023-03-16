using Microsoft.Extensions.DependencyInjection;
using Route256.PriceCalculator.Domain.Separated;
using Route256.PriceCalculator.Domain.Services;
using Route256.PriceCalculator.Infrastructure.Dal.Repositories;
using Route256.PriceCalculator.Infrastructure.External;

namespace Route256.PriceCalculator.Infrastructure;

public static class ServiceCollectionExstension
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IStorageRepository, StorageRepository>();
        services.AddSingleton<IGoodsRepository, GoodsRepository>();
        services.AddScoped<IGoodsService, GoodsService>();
        return services;
    }
}