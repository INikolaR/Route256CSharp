using System.Text.Json;
using Confluent.Kafka;

namespace Route256.Week6.Homework.PriceCalculator.BackgroundServices.Serializers;

public class JsonValueSerializer<T> : ISerializer<T>
{
    public byte[] Serialize(T data, SerializationContext context)
    {
        return JsonSerializer.SerializeToUtf8Bytes(data);
    }
}