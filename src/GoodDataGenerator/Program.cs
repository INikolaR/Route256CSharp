using System.Text;
using AutoFixture;
using Confluent.Kafka;
using Route256.Week6.Homework.PriceCalculator.BackgroundServices.Messages;
using Route256.Week6.Homework.PriceCalculator.BackgroundServices.Serializers;

const string broker = "localhost:9092";
const string topic = "good_price_calc_requests";
const int minSizeOfGood = -5;
const int maxSizeOfGood = 20;
const int anormalSizeOfGood = 1000;
const int minWeightOfGood = -10;
const int maxWeightOfGood = 2000;
const int anormalWeightOfGood = 2000000;
const int anormalProbability = 20;
const int badMessageProbability = 10;

using var producer = new ProducerBuilder<long, GoodMessage>(
        new ProducerConfig
        {
            BootstrapServers = broker,
            Acks = Acks.All
        })
    .SetValueSerializer(new JsonValueSerializer<GoodMessage>())
    .Build();

using var badProducer = new ProducerBuilder<long, int>(
        new ProducerConfig
        {
            BootstrapServers = broker,
            Acks = Acks.All
        })
    .SetValueSerializer(new JsonValueSerializer<int>())
    .Build();

var fixture = new Fixture();
var random = new Random();

while (true)
{
    Console.WriteLine("Input the number of messages or 'exit':");
    var command = Console.ReadLine() ?? "";
    if (command == "exit")
    {
        break;
    }

    try
    {
        var numberOfMessages = int.Parse(command);
        for (var i = 0; i < numberOfMessages; i++)
        {
            
            if (random.Next(badMessageProbability) == 0)
            {
                var message = fixture.Create<int>();
                await badProducer.ProduceAsync(
                    topic,
                    new Message<long, int>
                    {
                        Key = 100,
                        Value = message
                    });
            }
            else
            {
                var message = new GoodMessage(
                    random.Next(1, 1000000),
                    random.Next(anormalProbability) != 0
                        ? random.Next(minSizeOfGood, maxSizeOfGood)
                        : anormalSizeOfGood,
                    random.Next(anormalProbability) != 0
                        ? random.Next(minSizeOfGood, maxSizeOfGood)
                        : anormalSizeOfGood,
                    random.Next(anormalProbability) != 0
                        ? random.Next(minSizeOfGood, maxSizeOfGood)
                        : anormalSizeOfGood,
                    random.Next(anormalProbability) != 0
                        ? random.Next(minWeightOfGood, maxWeightOfGood)
                        : anormalWeightOfGood);
                await producer.ProduceAsync(
                    topic,
                    new Message<long, GoodMessage>
                    {
                        Key = message.GoodId,
                        Value = message
                    });
            }
        }
    }
    catch
    {
        Console.WriteLine("Bad input");
    }
}