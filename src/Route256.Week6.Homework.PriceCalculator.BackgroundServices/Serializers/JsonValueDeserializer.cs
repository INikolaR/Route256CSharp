using System.Runtime.Serialization;
using System.Text.Json;
using Confluent.Kafka;

namespace Route256.Week6.Homework.PriceCalculator.BackgroundServices.Serializers;

public class JsonValueDeserializer<T> : IDeserializer<T>
{
    private readonly IProducer<Null, byte[]> _producer;
    private readonly string _dlqTopic;
    public JsonValueDeserializer(IProducer<Null, byte[]> producer, string topic)
    {
        _producer = producer;
        _dlqTopic = topic;
    }
    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        if (isNull)
        {
            throw new ArgumentNullException(nameof(data), "Null data encountered");
        }

        try
        {
            return JsonSerializer.Deserialize<T>(data) ??
                   throw new ArgumentNullException(nameof(data), "Null data encountered");
        }
        catch
        {
            _producer.ProduceAsync(
                _dlqTopic, new Message<Null, byte[]>
                {
                    Value = data.ToArray()
                });
        }
        throw new SerializationException("Cannot deserialize message, it has been written to dlq topic");
    }
}