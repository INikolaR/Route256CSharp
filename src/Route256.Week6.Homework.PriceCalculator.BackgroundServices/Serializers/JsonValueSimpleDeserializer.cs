using System.Text.Json;
using Confluent.Kafka;

namespace Route256.Week6.Homework.PriceCalculator.BackgroundServices.Serializers;

public class JsonValueSimpleDeserializer<T> : IDeserializer<T>
{
    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        if (isNull)
        {
            throw new ArgumentNullException(nameof(data), "Null data encountered");
        }
        return JsonSerializer.Deserialize<T>(data) ??
               throw new ArgumentNullException(nameof(data), "Null data encountered");
    }
}