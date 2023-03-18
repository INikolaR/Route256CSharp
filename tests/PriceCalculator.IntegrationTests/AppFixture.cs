using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Route256.PriceCalculator.Api;

namespace PriceCalculator.IntegrationTests;

public class AppFixture : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddControllers().AddControllersAsServices();
        });
        base.ConfigureWebHost(builder);
    }
}