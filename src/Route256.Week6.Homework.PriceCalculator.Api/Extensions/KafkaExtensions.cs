using System.Reflection;
using Route256.Week5.Workshop.PriceCalculator.Bll.Services;
using Route256.Week5.Workshop.PriceCalculator.Bll.Services.Interfaces;
using Route256.Week5.Workshop.PriceCalculator.Dal.Settings;
using Route256.Week6.Homework.PriceCalculator.BackgroundServices.Settings;

namespace Route256.Week6.Homework.PriceCalculator.Api.Extensions;

public static class KafkaExtensions
{
    public static IServiceCollection AddKafka(
        this IServiceCollection services,
        IConfigurationRoot config)
    {
        services.Configure<KafkaOptions>(config.GetSection(nameof(KafkaOptions)));
        return services;
    }
}