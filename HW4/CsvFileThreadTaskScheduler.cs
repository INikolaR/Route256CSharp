using System.Collections.Concurrent;

namespace HW4;

public class CsvFileThreadTaskScheduler : TaskScheduler, IDisposable
{
    private object locker = new();
    
    private readonly BlockingCollection<Task> _queue = new ();

    private int _numberOfThreads;

    private List<Thread> _threads;

    private bool stopFlag = false;


    public CsvFileThreadTaskScheduler(int numberOfThreads)
    {
        if (numberOfThreads < 1)
            throw new ArgumentOutOfRangeException(nameof(numberOfThreads), "Must be at least 1");
        _numberOfThreads = numberOfThreads;
        _threads = new List<Thread>(16);
        for (int i = 0; i < 16; i++)
        {
            _threads.Add(new Thread(Run));
        }
        for (int i = 0; i < _numberOfThreads; i++)
        {
            _threads[i].Start();
        }
        Console.WriteLine($"Number of threads: {_numberOfThreads}");
    }

    private void Run()
    {
        while (!_queue.IsCompleted)
        {
            if (stopFlag)
            {
                Thread.CurrentThread.Abort();
            }
            if (_queue.TryTake(out Task task))
            {
                TryExecuteTask(task);
            }
        }
    }

    protected override IEnumerable<Task> GetScheduledTasks() => _queue.ToArray();

    protected override void QueueTask(Task task)
    {
        _queue.Add(task);
    }

    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
        return false;
    }

    public void Dispose()
    {
        _queue.CompleteAdding();

        for (var i = 0; i < _numberOfThreads; ++i)
        { 
            if (_threads[i].ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
                _threads[i].Join();
        }
    }

    public void ChangeNumberOfThreads(int numberOfThreads)
    {

        lock (locker)
        {
            _numberOfThreads = numberOfThreads;
            for (int i = 0; i < _numberOfThreads; i++)
            {
                _threads[i] = new Thread(Run);
                _threads[i].Start();
            }
            Console.WriteLine($"Number of threads: {_numberOfThreads}");
        }
    }
}