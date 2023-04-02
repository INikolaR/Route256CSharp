using System.Threading.Channels;

namespace HW4;

public class OutputWriter : IDisposable
{
    private readonly Channel<string> _channel;
    private readonly int _channelBoud = 1000;
    private readonly StreamWriter _writer;

    public int NumberOfLinesWritten = 0;
    
    public Channel<string> StreamChannel => _channel;

    public OutputWriter(string outputPath)
    {
        // channel between counter and output file.
        _channel = Channel.CreateBounded<string>(_channelBoud);
        
        _writer = new StreamWriter(outputPath);
    }

    public async Task Start()
    {
        await _writer.WriteLineAsync("id, delivery_price");
        Task.Run(async () =>
        {
            await foreach (var line in _channel.Reader.ReadAllAsync())
            {
                Interlocked.Increment(ref NumberOfLinesWritten);
                await _writer.WriteLineAsync(line);
            }
        });
    }
    
    public void Dispose()
    {
        _writer.Dispose();
    }
}