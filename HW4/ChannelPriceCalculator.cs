using System.Globalization;
using System.Threading.Channels;

namespace HW4;

public class ChannelPriceCalculator
{
    private readonly Channel<string> _inputChannel;
    private readonly Channel<string> _outputChannel;
    private readonly CsvFileThreadTaskScheduler _scheduler;

    public int NumberOfResultsCounted = 0;
    private readonly InputReader _reader;
    private readonly OutputWriter _writer;

    public ChannelPriceCalculator(Channel<string> inputChannel,
                                  Channel<string> outputChannel,
                                  CsvFileThreadTaskScheduler scheduler,
                                  InputReader reader,
                                  OutputWriter writer)
    {
        _inputChannel = inputChannel;
        _outputChannel = outputChannel;
        _scheduler = scheduler;
        _reader = reader;
        _writer = writer;
    }
    
    /// <summary>
    /// Parses a CSV string.
    /// </summary>
    /// <param name="line">Input string.</param>
    /// <returns>Good object.</returns>
    private static Good Parse(string line)
    {
        var dataArray = Array.ConvertAll(line.Replace(" ", "").Split(','), int.Parse);
        return new Good(dataArray[0], dataArray[1], dataArray[2], dataArray[3], dataArray[4]);
    }


    public async Task Start()
    {
        var l = new List<Task>();
        await foreach (var line in _inputChannel.Reader.ReadAllAsync())
        {
            l.Add(Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        Interlocked.Increment(ref NumberOfResultsCounted);
                        var good = Parse(line);
                        await _outputChannel.Writer
                            .WriteAsync(good.Id + ", " +
                                        PriceCalculator.CalculatePrice(good).ToString(CultureInfo.InvariantCulture));
                        Console.WriteLine("" +
                                          "Lines read:" + _reader.NumberOfLinesRead +
                                          " Results counted:" + NumberOfResultsCounted +
                                          " Lines written:" + _writer.NumberOfLinesWritten);
                    }
                    catch
                    {
                        Console.WriteLine("Csv file is corrupted, omitting incorrect-formatted strings...");
                    }
                },
                CancellationToken.None,
                TaskCreationOptions.None,
                _scheduler));
        }
        await Task.WhenAll(l);
    }
}