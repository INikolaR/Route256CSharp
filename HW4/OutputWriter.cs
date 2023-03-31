using System.Threading.Channels;

namespace HW4;

public class OutputWriter : IDisposable
{
    private readonly Channel<string> _channel;
    private readonly string _outputPath;
    private readonly StreamWriter _writer;

    public int NumberOfLinesWritten = 0;

    public OutputWriter(Channel<string> channel, string outputPath)
    {
        _channel = channel;
        _outputPath = outputPath;
        _writer = new StreamWriter(_outputPath);
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