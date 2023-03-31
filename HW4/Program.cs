using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Channels;

namespace HW4;

public class Program
{
    private static string _inputPath;
    private static string _outputPath;
    private static int _numberOfThreads;
    private const string PathToConfig = "../../../config.txt";
    private static int _numberOfLinesRead = 0;
    private static int _numberOfResultsCounted = 0;
    private static int _numberOfLinesWritten = 0;
    private static readonly object Locker = new object(); // locks all three status counters.

    private static event Action? ChangedProgressStatus; // if one of status counters changed.

    private static int NumberOfLinesRead
    {
        get => _numberOfLinesRead;
        set
        {
            _numberOfLinesRead = value;
            ChangedProgressStatus?.Invoke();
        }
    }
    
    private static int NumberOfLinesWritten
    {
        get => _numberOfLinesWritten;
        set
        {
            _numberOfLinesWritten = value;
            ChangedProgressStatus?.Invoke();
        }
    }
    
    private static int NumberOfResultsCounted
    {
        get => _numberOfResultsCounted;
        set
        {
            _numberOfResultsCounted = value;
            ChangedProgressStatus?.Invoke();
        }
    }
    
    /// <summary>
    /// Checks if filename is correct.
    /// </summary>
    /// <param name="name">The name of file.</param>
    /// <param name="modeToCheck"></param>
    /// <returns></returns>
    private static bool IsFileNameCorrect(string name, FileMode modeToCheck)
    {
        try
        {
            FileStream fs = File.Open(name, modeToCheck);
            fs.Close();
        }
        catch (ArgumentException)
        {
            Console.WriteLine("Bad filename");
            return false;
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("File not found");
            return false;
        }
        catch (IOException)
        {
            Console.WriteLine("Cannot access file");
            return false;
        }
        return true;
    }


    // 
    /// <summary>
    /// Asks user to input a path to something.
    /// </summary>
    /// <param name="message">Message to print on screen.</param>
    /// <param name="modeToCheck">File mode to try to open the file with specified path with.</param>
    /// <param name="path">Path to file.</param>
    private static void GetPath(string message, FileMode modeToCheck, out string path)
    {
        do
        {
            Console.WriteLine(message);
            path = Console.ReadLine() ?? string.Empty;
        } while (!IsFileNameCorrect(path, modeToCheck));
    }
    
    /// <summary>
    /// Reads config to set the number of threads.
    /// </summary>
    /// <returns></returns>
    private static bool GetNumberOfThreadsFromConfig()
    {
        try
        {
            _numberOfThreads = int.Parse(File.ReadAllText(PathToConfig));
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

    private static async IAsyncEnumerable<string?> GetDataFromFile(StreamReader reader)
    {
        while (!reader.EndOfStream)
        {
            yield return await reader.ReadLineAsync();
        }
    }

    public static async Task Main(string[] args)
    {
        // asking to specify paths to input/output files.
        GetPath("Print path to input file:", FileMode.Open, out _inputPath);
        GetPath("Print path to output file:", FileMode.OpenOrCreate, out _outputPath);
        // reading config.
        if (!GetNumberOfThreadsFromConfig())
        {
            return;
        }

        // adding method to print progress if it is changed.
        ChangedProgressStatus += () =>
        {
            lock (Locker)
            {
                Console.WriteLine("" +
                                  "Lines read:" + NumberOfLinesRead +
                                  " Results counted:" + NumberOfResultsCounted +
                                  " Lines written:" + NumberOfLinesWritten);
            }
        };
        
        using var scheduler = new CsvFileThreadTaskScheduler(_numberOfThreads);

        // channel between input file and counter.
        var inputChannel = Channel.CreateBounded<string>(1000);
        // channel between counter and output file.
        var outputChannel = Channel.CreateBounded<string>(1000);

        // starting reading.
        using var reader = new StreamReader(_inputPath);
        await reader.ReadLineAsync();
        Task.Run(async () =>
        {
            await foreach (var line in GetDataFromFile(reader))
            {
                lock (Locker)
                {
                    ++NumberOfLinesRead;
                }
                await inputChannel.Writer.WriteAsync(line);
            }

            inputChannel.Writer.Complete();
        });

        // starting writing.
        await using var writer = new StreamWriter(_outputPath); 
        await writer.WriteLineAsync("id, delivery_price");
        Task.Run(async () =>
        {
            await foreach (var line in outputChannel.Reader.ReadAllAsync())
            {
                lock (Locker)
                {
                    ++NumberOfLinesWritten;
                }
                await writer.WriteLineAsync(line);
            }
        });

        // starting counting.
        var l = new List<Task>();
        await foreach (var line in inputChannel.Reader.ReadAllAsync())
        {
            l.Add(Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        lock (Locker)
                        {
                            ++NumberOfResultsCounted;
                        }
                        var good = Parse(line);
                        await outputChannel.Writer
                            .WriteAsync(good.Id + ", " +
                                        PriceCalculator.CalculatePrice(good).ToString(CultureInfo.InvariantCulture));
                    }
                    catch
                    {
                        Console.WriteLine("Csv file is corrupted, omitting incorrect-formatted strings...");
                    }
                },
                CancellationToken.None,
                TaskCreationOptions.None,
                scheduler));
        }
        await Task.WhenAll(l);
    }
}