namespace Route256.PriceCalculator.Api;

public sealed class Program
{
    public static void Main()
    {
        var builder = Host
            .CreateDefaultBuilder()
            .ConfigureWebHostDefaults(x => x.UseStartup<Startup>());
        
        builder.Build().Run();
    }
}