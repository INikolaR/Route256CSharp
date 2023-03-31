using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Channels;

namespace HW4;

public class Program
{
    private const string _inputPath = "../../../input.txt";
    private const string _outputPath = "../../../output.txt";
    private static int _numberOfThreads;
    private const string PathToConfig = "../../../config.txt";
    private const string PathToConfigFolder = "../../../";

    public static event Action? ChangedProgressStatus; // if one of status counters changed.

    /// <summary>
    /// Reads config to set the number of threads.
    /// </summary>
    /// <returns></returns>
    private static bool GetNumberOfThreadsFromConfig()
    {
        try
        {
            var numberOfThreads = int.Parse(File.ReadAllText(PathToConfig));
            if (numberOfThreads is < 0 or > 16)
            {
                throw new ArgumentException("Number of threads must be between 1 and 16");
            }
            _numberOfThreads = numberOfThreads;
        }
        catch (ArgumentException)
        {
            Console.WriteLine("Cannot access config file");
            return false;
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("Cannot access config file");
            return false;
        }
        catch (IOException)
        {
            Console.WriteLine("Cannot access config file");
            return false;
        }
        catch (Exception)
        {
            Console.WriteLine("Cannot parse data from config file");
            return false;
        }
        return true;
    }

    
    public static async Task Main(string[] args)
    {
        if (!GetNumberOfThreadsFromConfig())
        {
            return;
        }
        
        using var scheduler = new CsvFileThreadTaskScheduler(_numberOfThreads);
        using var watcher = new FileSystemWatcher(PathToConfigFolder);
        watcher.Changed += (sender, eventArgs) =>
        {
            if (GetNumberOfThreadsFromConfig())
            {
                scheduler.ChangeNumberOfThreads(_numberOfThreads);
            }
        };
        watcher.EnableRaisingEvents = true;
        watcher.Filter = "config.txt";
        
        
        
        // channel between input file and counter.
        var inputChannel = Channel.CreateBounded<string>(1000);
        // channel between counter and output file.
        var outputChannel = Channel.CreateBounded<string>(1000);
        
        using var reader = new InputReader(inputChannel, _inputPath);
        using var writer = new OutputWriter(outputChannel, _outputPath);
        var calculator = new ChannelPriceCalculator(inputChannel, outputChannel, scheduler, reader, writer);

        await Task.WhenAll(reader.Start(), writer.Start(), calculator.Start());
    }
}