using System.Threading.Channels;

namespace HW4;

public class InputReader : IDisposable
{
    private readonly Channel<string> _channel;
    private readonly int _channelBound = 1000;
    private readonly StreamReader _reader;

    public int NumberOfLinesRead = 0;

    public Channel<string> StreamChannel => _channel;

    public InputReader(string inputPath)
    {
        // channel between input file and counter.
        _channel = Channel.CreateBounded<string>(_channelBound);
        
        _reader = new StreamReader(inputPath);
    }
    
    private static async IAsyncEnumerable<string?> GetDataFromFile(StreamReader reader)
    {
        while (!reader.EndOfStream)
        {
            yield return await reader.ReadLineAsync();
        }
    }

    public async Task Start()
    {
        await _reader.ReadLineAsync();
        Task.Run(async () =>
        {
            await foreach (var line in GetDataFromFile(_reader))
            {
                Thread.Sleep(100);
                Interlocked.Increment(ref NumberOfLinesRead);
                await _channel.Writer.WriteAsync(line);
            }
            _channel.Writer.Complete();
        });
    }
    
    public void Dispose()
    {
        _reader.Dispose();
    }
}