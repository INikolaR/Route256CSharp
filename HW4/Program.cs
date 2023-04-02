using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Channels;

namespace HW4;

public class Program
{
    private const string _inputFile = "input.txt";
    private const string _outputFile = "output.txt";
    private static int _numberOfThreads;
    private const string configFile = "config.txt";
    private const string PathToConfigFolder = "../../../";

    /// <summary>
    /// Reads config to set the number of threads.
    /// </summary>
    /// <returns></returns>
    private static bool GetNumberOfThreadsFromConfig(string pathToConfig)
    {
        try
        {
            var numberOfThreads = int.Parse(File.ReadAllText(pathToConfig));
            if (numberOfThreads is < 1 or > 16)
            {
                throw new ArgumentException("Number of threads must be between 1 and 16");
            }

            _numberOfThreads = numberOfThreads;
        }
        catch (ArgumentException e)
        {
            Console.WriteLine(e.Message);
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
        string pathToConfig = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", configFile);
        if (!GetNumberOfThreadsFromConfig(pathToConfig))
        {
            return;
        }
        
        using var scheduler = new CsvFileThreadTaskScheduler(_numberOfThreads);
        using var watcher = new FileSystemWatcher(PathToConfigFolder);
        watcher.Changed += (sender, eventArgs) =>
        {
            if (GetNumberOfThreadsFromConfig(pathToConfig))
            {
                scheduler.ChangeNumberOfThreads(_numberOfThreads);
            }
        };
        watcher.EnableRaisingEvents = true;
        watcher.Filter = configFile;

        var inputPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", _inputFile);
        var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", _outputFile);
        using var reader = new InputReader(inputPath);
        using var writer = new OutputWriter(outputPath);
        var calculator = new ChannelPriceCalculator(scheduler, reader, writer);

        await Task.WhenAll(reader.Start(), writer.Start(), calculator.Start());
    }
}