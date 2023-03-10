using System.Net;
using Microsoft.AspNetCore.Mvc;
using Route256.PriceCalculator.Api.ActionFilters;
using Route256.PriceCalculator.Api.Bll;
using Route256.PriceCalculator.Api.Bll.Services;
using Route256.PriceCalculator.Api.Bll.Services.Interfaces;
using Route256.PriceCalculator.Api.Dal.Repositories;
using Route256.PriceCalculator.Api.Dal.Repositories.Interfaces;
using Route256.PriceCalculator.Api.HostedServices;

namespace Route256.PriceCalculator.Api;

public sealed class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc()
            .AddMvcOptions(x =>
            {
                x.Filters.Add(new ExceptionFilterAttribute());
                x.Filters.Add(new ResponseTypeAttribute((int)HttpStatusCode.InternalServerError));
                x.Filters.Add(new ResponseTypeAttribute((int)HttpStatusCode.BadRequest));
                x.Filters.Add(new ProducesResponseTypeAttribute((int)HttpStatusCode.OK));
            });
        
        services.Configure<PriceCalculatorOptions>(_configuration.GetSection("PriceCalculatorOptions"));
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(o =>
        {
            o.CustomSchemaIds(x => x.FullName);
        });
        
        services.AddScoped<IPriceCalculatorService, PriceCalculatorService>();
        services.AddScoped<IGoodPriceCalculatorService, GoodPriceCalculatorService>();
        services.AddHostedService<GoodsSyncHostedService>();
        services.AddSingleton<IStorageRepository, StorageRepository>();
        services.AddSingleton<IGoodsRepository, GoodsRepository>();
        services.AddScoped<IGoodsService, GoodsService>();
        services.AddHttpContextAccessor();
    }

    public void Configure(
        IHostEnvironment environment,
        IApplicationBuilder app)
    {

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}