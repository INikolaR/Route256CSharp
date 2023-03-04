namespace Route256.Week1.Homework.PriceCalculator.Api;

internal sealed class Program
{
    public static void Main()
    {
        var builder = Host
            .CreateDefaultBuilder()
            .ConfigureWebHostDefaults(x => x.UseStartup<Startup>());
        
        builder.Build().Run();
    }
}