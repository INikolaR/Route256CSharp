// See https://aka.ms/new-console-template for more information

using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureServices(services =>
    {
        services
            .AddGrpcClient<Calculator.CalculatorClient>(o =>
            {
                o.Address = new Uri("http://localhost:5273");
            });
    }).Build();
await host.StartAsync();

await RunAsync(host.Services.GetRequiredService<Calculator.CalculatorClient>());

await host.StopAsync();

async Task RunAsync(Calculator.CalculatorClient client)
{
    var information =
        "Print the name of method (CalculatePrice, GetHistory, ClearHistory or CalculatePriceStream) or exit:";
    Console.WriteLine(information);
    var input = Console.ReadLine();
    while (input != "exit")
    {
        switch (input)
        {
            case "CalculatePrice":
                await HandleCalculatePrice(client);
                break;
            case "GetHistory":
                await HandleGetHistory(client);
                break;
            case "ClearHistory":
                await HandleClearHistory(client);
                break;
            case "CalculatePriceStream":
                await HandleCalculatePriceStream(client);
                break;
            case "exit":
                break;
            default:
                Console.WriteLine("Bad command!");
                break;
        }
        Console.WriteLine(information);
        input = Console.ReadLine();
    }
}

async Task HandleCalculatePrice(Calculator.CalculatorClient client)
{
    Console.WriteLine("Input parameters int the following format:");
    Console.WriteLine("<user_id> <number_of_goods> <length> <width> <height> <weight> ... <length> <width> <height> <weight>");
    try
    {
        var parameters = Console.ReadLine() ?? "";
        var request = ParseCalculationRequest(parameters);
        var response = await client.ProtoCalculatePriceAsync(request);
        Console.WriteLine(response.ToString());
    }
    catch
    {
        Console.WriteLine("Bad parameters");
    }
}

async Task HandleGetHistory(Calculator.CalculatorClient client)
{
    Console.WriteLine("Input user_id:");
    try
    {
        var userId = int.Parse(Console.ReadLine() ?? "");
        var call = client.ProtoGetHistory(new ProtoGetHistoryRequest
        {
            UserId = userId
        });
        await foreach (var response in call.ResponseStream.ReadAllAsync())
        {
            Console.WriteLine(response.ToString());
        }
    }
    catch
    {
        Console.WriteLine("Bad input");
    }
}

async Task HandleClearHistory(Calculator.CalculatorClient client)
{
    Console.WriteLine("Input parameters int the following format:");
    Console.WriteLine("<user_id> <number_of_calculation_ids> <calculation_id> ... <calculation_id>");
    try
    {
        var parameters = Console.ReadLine() ?? "";
        var request = ParseClearHistoryRequest(parameters);
        await client.ProtoClearHistoryAsync(request);
    }
    catch
    {
        Console.WriteLine("Bad input");
    }
}

async Task HandleCalculatePriceStream(Calculator.CalculatorClient client)
{
    Console.WriteLine("Input path to a file with strings of the following format:");
    Console.WriteLine("<user_id> <number_of_goods> <length> <width> <height> <weight> ... <length> <width> <height> <weight>");
    var path = Console.ReadLine() ?? "";
    try
    {
        var call = client.ProtoBidirectionalStreamingCalculatePrice();
        var responseTask = Task.Run(async () =>
        {
            await foreach (var response in call.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine(response.ToString());
            }
        });
        using var reader = new StreamReader(path);
        while (!reader.EndOfStream)
        {
            var str = await reader.ReadLineAsync() ?? "";
            try
            {
                var request = ParseCalculationRequest(str);
                await call.RequestStream.WriteAsync(request);
            }
            catch
            {
                Console.WriteLine("Bad input");
            }
        }
        await call.RequestStream.CompleteAsync();
        await responseTask;
    }
    catch
    {
        Console.WriteLine("Bad input");
    }
}

ProtoCalculationRequest ParseCalculationRequest(string input)
{
    var parameters = Array.ConvertAll(input.Split(' '), int.Parse);
    var userId = parameters[0];
    var number = parameters[1];
    var goods = new List<ProtoGood>();
    for (int i = 2; i < number * 4 + 2; i += 4)
    {
        goods.Add(new ProtoGood
        {
            Length = parameters[i],
            Width = parameters[i + 1],
            Height = parameters[i + 2],
            Weight = parameters[i + 3]
        });
    }

    return new ProtoCalculationRequest
    {
        UserId = userId,
        Goods = { goods }
    };
}

ProtoClearHistoryRequest ParseClearHistoryRequest(string input)
{
    var parameters = Array.ConvertAll(input.Split(' '), int.Parse);
    var userId = parameters[0];
    var number = parameters[1];
    var calculationIds = new List<long>();
    for (int i = 0; i < number; ++i)
    {
        calculationIds.Add(parameters[i + 2]);
    }
    return new ProtoClearHistoryRequest
    {
        UserId = userId,
        CalculationIds = { calculationIds }
    };
}