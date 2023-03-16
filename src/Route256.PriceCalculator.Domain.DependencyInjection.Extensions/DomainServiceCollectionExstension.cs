using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Route256.PriceCalculator.Domain.Separated;
using Route256.PriceCalculator.Domain.Services;
using Route256.PriceCalculator.Domain.Services.Interfaces;

namespace Route256.PriceCalculator.Domain.DependencyInjection.Extensions;

public static class DomainServiceCollectionExstension
{
    public static IServiceCollection AddDomain(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PriceCalculatorOptions>(configuration.GetSection("PriceCalculatorOptions"));
        services.AddScoped<IPriceCalculatorService, PriceCalculatorService>(x =>
        {
            var options = x.GetRequiredService<IOptionsSnapshot<PriceCalculatorOptions>>().Value;
            return new PriceCalculatorService(options, x.GetRequiredService<IStorageRepository>());
        });
        services.AddScoped<IGoodPriceCalculatorService, GoodPriceCalculatorService>();
        return services;
    }
}