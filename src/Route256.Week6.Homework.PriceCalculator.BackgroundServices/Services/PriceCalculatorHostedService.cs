using System.Threading.Channels;
using Confluent.Kafka;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Route256.Week6.Homework.PriceCalculator.BackgroundServices.Commands;
using Route256.Week6.Homework.PriceCalculator.BackgroundServices.Messages;
using Route256.Week6.Homework.PriceCalculator.BackgroundServices.Serializers;
using Route256.Week6.Homework.PriceCalculator.BackgroundServices.Settings;

namespace Route256.Week6.Homework.PriceCalculator.BackgroundServices.Services;

public class PriceCalculatorHostedService : IHostedService, IDisposable
{
    private const int InputChannelBufferSize = 100;
    private const int OutputChannelBufferSize = 100;
    private readonly KafkaOptions _kafkaOptions;
    private readonly Channel<GoodMessage> _inputChannel = Channel.CreateBounded<GoodMessage>(
        new BoundedChannelOptions(InputChannelBufferSize)
        {
            SingleWriter = true,
            SingleReader = true,
            AllowSynchronousContinuations = true,
            FullMode = BoundedChannelFullMode.Wait
        });
    private readonly Channel<PriceMessage> _outputChannel = Channel.CreateBounded<PriceMessage>(
        new BoundedChannelOptions(OutputChannelBufferSize)
        {
            SingleWriter = true,
            SingleReader = true,
            AllowSynchronousContinuations = true,
            FullMode = BoundedChannelFullMode.Wait
        });
    private readonly IProducer<long, PriceMessage> _producer;
    private readonly IConsumer<long, GoodMessage> _consumer;
    private readonly IProducer<long, GoodMessage> _dlqProducer;
    private readonly IMediator _mediator;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public PriceCalculatorHostedService(
        IMediator mediator,
        IOptionsMonitor<KafkaOptions> options)
    {
        _mediator = mediator;
        _kafkaOptions = options.CurrentValue;
        _producer = new ProducerBuilder<long, PriceMessage>(
                new ProducerConfig
                {
                    BootstrapServers = _kafkaOptions.Broker,
                    Acks = Acks.All
                })
            .SetValueSerializer(new JsonValueSerializer<PriceMessage>())
            .Build();
        _dlqProducer = new ProducerBuilder<long, GoodMessage>(
                new ProducerConfig
                {
                    BootstrapServers = _kafkaOptions.Broker,
                    Acks = Acks.All
                })
            .SetValueSerializer(new JsonValueSerializer<GoodMessage>())
            .Build();
        _consumer = new ConsumerBuilder<long, GoodMessage>(
                new ConsumerConfig
                {
                    BootstrapServers = _kafkaOptions.Broker,
                    GroupId = "good-calculator-service-listener",
                    AutoOffsetReset = AutoOffsetReset.Earliest
                })
            .SetValueDeserializer(
                new JsonValueDeserializer<GoodMessage>(
                    new ProducerBuilder<Null, byte[]>(new ProducerConfig
                        {
                            BootstrapServers = _kafkaOptions.Broker,
                            Acks = Acks.All
                        })
                        .SetValueSerializer(new ByteValueSerializer())
                        .Build(),
                    _kafkaOptions.DlqTopic))
            .Build();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var consumeTask = Task.Factory.StartNew(() => Consume(_inputChannel.Writer, _consumer, _cancellationTokenSource.Token), TaskCreationOptions.LongRunning).Unwrap();
        var reportTask = Task.Factory.StartNew(() => Report(_outputChannel.Reader, _producer, _cancellationTokenSource.Token), TaskCreationOptions.LongRunning).Unwrap();
        var handleTask = Task.Factory.StartNew(() => Handle(_inputChannel.Reader, _outputChannel.Writer, _cancellationTokenSource.Token), TaskCreationOptions.LongRunning).Unwrap();
        Thread thread = new Thread(() => Task.WhenAll(consumeTask, reportTask, handleTask));
        thread.Start();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.Run(() => _cancellationTokenSource.Cancel(), cancellationToken);
    }

    private async Task Consume(
        ChannelWriter<GoodMessage> channelWriter,
        IConsumer<long, GoodMessage> consumer,
        CancellationToken cancellationToken)
    {
        consumer.Subscribe(_kafkaOptions.InputTopic);
        
        while (await GetNextCorrectMessageAsync(consumer, cancellationToken) is { } result)
        {
            await channelWriter.WriteAsync(result.Message.Value, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task<ConsumeResult<long, GoodMessage>> GetNextCorrectMessageAsync(
        IConsumer<long, GoodMessage> consumer,
        CancellationToken cancellationToken)
    {
        ConsumeResult<long, GoodMessage> result = null;
        while (true)
        {
            try
            {
                result = consumer.Consume();
                if (!VerifyMessage(result.Message.Value))
                {
                    await _dlqProducer.ProduceAsync(
                        _kafkaOptions.DlqTopic,
                        new Message<long, GoodMessage>
                        {
                            Key = result.Message.Key,
                            Value = result.Message.Value
                        }, cancellationToken);
                    throw new ValidationException("Message did not pass validation");
                }
                break;
            }
            catch (Exception e)
            {
                // the fact that we are here means we have got a bad message and have to consume another one.
                continue;
            }
        }
        return result;
    }

    private async Task Handle(
        ChannelReader<GoodMessage> channelReader,
        ChannelWriter<PriceMessage> channelWriter,
        CancellationToken cancellationToken)
    {
        await foreach (var message in channelReader.ReadAllAsync(cancellationToken))
        {
            var result = await _mediator.Send(new CalculateDeliveryPriceHostedCommand(message), cancellationToken);
            await channelWriter.WriteAsync(result, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
    
    private async Task Report(
        ChannelReader<PriceMessage> channelReader,
        IProducer<long, PriceMessage> producer,
        CancellationToken cancellationToken)
    {
        await foreach (var priceMessage in channelReader.ReadAllAsync(cancellationToken))
        {
            await producer.ProduceAsync(
                _kafkaOptions.OutputTopic,
                new Message<long, PriceMessage>
                {
                    Key = priceMessage.GoodId,
                    Value = priceMessage
                }, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private bool VerifyMessage(GoodMessage message)
    {
        return message.Height >= 0
               && message.Width >= 0
               && message.Length >= 0
               && message.Weight >= 0;
    }

    public void Dispose()
    {
        _consumer.Dispose();
        _producer.Dispose();
    }
}