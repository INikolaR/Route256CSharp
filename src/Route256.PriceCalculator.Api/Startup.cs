using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Route256.PriceCalculator.Api.ActionFilters;
using Route256.PriceCalculator.Api.HostedServices;
using Route256.PriceCalculator.Domain;
using Route256.PriceCalculator.Domain.DependencyInjection.Extensions;
using Route256.PriceCalculator.Domain.Separated;
using Route256.PriceCalculator.Domain.Services;
using Route256.PriceCalculator.Domain.Services.Interfaces;
using Route256.PriceCalculator.Infrastructure;
using Route256.PriceCalculator.Infrastructure.Dal.Repositories;

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
        

        services
            .AddDomain(_configuration)
            .AddInfrastructure()
            .AddControllers()
            .AddMvcOptions(ConfigureMvc)
            .Services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen(o =>
            {
                o.CustomSchemaIds(x => x.FullName);
            })
            .AddHostedService<GoodsSyncHostedService>()
            .AddHttpContextAccessor();
    }
    
    private void ConfigureMvc(MvcOptions x)
    {
        x.Filters.Add(new ExceptionFilterAttribute());
        x.Filters.Add(new ResponseTypeAttribute((int)HttpStatusCode.InternalServerError));
        x.Filters.Add(new ResponseTypeAttribute((int)HttpStatusCode.BadRequest));
        x.Filters.Add(new ProducesResponseTypeAttribute((int)HttpStatusCode.OK));
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