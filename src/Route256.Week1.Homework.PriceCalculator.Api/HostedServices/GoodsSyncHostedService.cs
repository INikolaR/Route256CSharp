using Microsoft.Extensions.Options;
using Route256.Week1.Homework.PriceCalculator.Api.Bll;
using Route256.Week1.Homework.PriceCalculator.Api.Bll.Services.Interfaces;
using Route256.Week1.Homework.PriceCalculator.Api.Dal.Repositories.Interfaces;

namespace Route256.Week1.Homework.PriceCalculator.Api.HostedServices;

public sealed class GoodsSyncHostedService: BackgroundService
{
    private readonly IGoodsRepository _repository;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsMonitor<GoodsSyncHostedOptions> _options;

    public GoodsSyncHostedService(
        IOptionsMonitor<GoodsSyncHostedOptions> options,
        IGoodsRepository repository,
        IServiceProvider serviceProvider)
    {
        _options = options;
        _repository = repository;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var goodsService = scope.ServiceProvider.GetRequiredService<IGoodsService>();
                var goods = goodsService.GetGoods().ToList();
                foreach (var good in goods)
                    _repository.AddOrUpdate(good);
            }
            // Через _options.CurrentValue.SecondsDelay получается доступ к данным из конфига.
            await Task.Delay(TimeSpan.FromSeconds(_options.CurrentValue.SecondsDelay), stoppingToken);
        }
    }
}