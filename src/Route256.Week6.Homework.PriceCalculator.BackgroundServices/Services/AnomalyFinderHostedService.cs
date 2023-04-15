using System.Threading.Channels;
using Confluent.Kafka;
using Dapper;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;
using Route256.Week5.Workshop.PriceCalculator.Dal.Entities;
using Route256.Week5.Workshop.PriceCalculator.Dal.Settings;
using Route256.Week6.Homework.PriceCalculator.BackgroundServices.Messages;
using Route256.Week6.Homework.PriceCalculator.BackgroundServices.Serializers;
using Route256.Week6.Homework.PriceCalculator.BackgroundServices.Settings;

namespace Route256.Week6.Homework.PriceCalculator.BackgroundServices.Services;

public class AnomalyFinderHostedService : IHostedService, IDisposable
{
    private const int maxPrice = 40000;
    private const int ChannelBufferSize = 100;
    private readonly KafkaOptions _kafkaOptions;
    private readonly Channel<PriceMessage> _channel = Channel.CreateBounded<PriceMessage>(
        new BoundedChannelOptions(ChannelBufferSize)
        {
            SingleWriter = true,
            SingleReader = true,
            AllowSynchronousContinuations = true,
            FullMode = BoundedChannelFullMode.Wait
        });
    private readonly IConsumer<long, PriceMessage> _consumer;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly DalOptions _dalSettings;

    public AnomalyFinderHostedService(
        IOptionsMonitor<DalOptions> dalSettings,
        IOptionsMonitor<KafkaOptions> options)
    {
        _dalSettings = dalSettings.CurrentValue;
        _kafkaOptions = options.CurrentValue;
        _consumer = _consumer = new ConsumerBuilder<long, PriceMessage>(
                new ConsumerConfig
                {
                    BootstrapServers = _kafkaOptions.Broker,
                    GroupId = "good-calculator-service-analyser",
                    AutoOffsetReset = AutoOffsetReset.Earliest
                })
            .SetValueDeserializer(new JsonValueSimpleDeserializer<PriceMessage>())
            .Build();
        _cancellationTokenSource = new CancellationTokenSource();
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var consumeTask = Task.Factory.StartNew(() => Consume(_channel.Writer, _consumer, _cancellationTokenSource.Token), TaskCreationOptions.LongRunning).Unwrap();
        var handleTask = Task.Factory.StartNew(() => Handle(_channel.Reader, _cancellationTokenSource.Token), TaskCreationOptions.LongRunning).Unwrap();
        Thread thread = new Thread(() => Task.WhenAll(consumeTask, handleTask));
        thread.Start();
    }
    
    private async Task Consume(
        ChannelWriter<PriceMessage> channelWriter,
        IConsumer<long, PriceMessage> consumer,
        CancellationToken cancellationToken)
    {
        consumer.Subscribe(_kafkaOptions.OutputTopic);
        
        while (consumer.Consume() is { } result)
        {
            await channelWriter.WriteAsync(result.Message.Value, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
    
    private async Task Handle(
        ChannelReader<PriceMessage> channelReader,
        CancellationToken cancellationToken)
    {
        await foreach (var message in channelReader.ReadAllAsync(cancellationToken))
        {
            if (message.Price > maxPrice)
            {
                await AddToDatabase(new PriceAnomalyEntityV1
                {
                    GoodId = message.GoodId,
                    Price = message.Price
                }, cancellationToken);
            }
        }
    }
    
    private async Task AddToDatabase(
        PriceAnomalyEntityV1 entityV1, 
        CancellationToken token)
    {
        const string sqlQuery = @"
insert into price_anomalies (good_id, price) values (@GoodId, @Price);
";
        
        var sqlQueryParams = new
        {
            GoodId = entityV1.GoodId,
            Price = entityV1.Price
        };
        
        await using var connection = await GetAndOpenConnection();
        await connection.QueryAsync(
            new CommandDefinition(
                sqlQuery,
                sqlQueryParams,
                cancellationToken: token));
    }
    
    private async Task<NpgsqlConnection> GetAndOpenConnection()
    {
        var connection = new NpgsqlConnection(_dalSettings.ConnectionString);
        await connection.OpenAsync();
        connection.ReloadTypes();
        return connection;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.Run(() => _cancellationTokenSource.Cancel(), cancellationToken);
    }

    public void Dispose()
    {
        _consumer.Dispose();
    }
}