using System.Reflection;
using Route256.Week5.Workshop.PriceCalculator.Bll.Services;
using Route256.Week5.Workshop.PriceCalculator.Bll.Services.Interfaces;
using Route256.Week6.Homework.PriceCalculator.BackgroundServices.Services;

namespace Route256.Week6.Homework.PriceCalculator.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBll(this IServiceCollection services)
    {
        services.AddMediatR(c => c.RegisterServicesFromAssemblies(new Assembly[]
        {
            typeof(CalculationService).Assembly,
            typeof(PriceCalculatorHostedService).Assembly
        }));
        services.AddTransient<ICalculationService, CalculationService>();
        services.AddHostedService<PriceCalculatorHostedService>();
        services.AddHostedService<AnomalyFinderHostedService>();
        return services;
    }
}