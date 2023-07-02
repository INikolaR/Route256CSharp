using Confluent.Kafka;

namespace Route256.Week6.Homework.PriceCalculator.BackgroundServices.Serializers;

public class ByteValueSerializer : ISerializer<byte[]>
{
    public byte[] Serialize(byte[] data, SerializationContext context)
    {
        return data;
    }
}