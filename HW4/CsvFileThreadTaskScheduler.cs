using System.Collections.Concurrent;

namespace HW4;

public class CsvFileThreadTaskScheduler : TaskScheduler, IDisposable
{
    private object locker = new();
    
    private readonly BlockingCollection<Task> _queue = new ();

    private int _numberOfThreads;

    private Thread[] _threads;

    private Dictionary<int, int> _indexes = new();


    public CsvFileThreadTaskScheduler(int numberOfThreads)
    {
        if (numberOfThreads < 1)
            throw new ArgumentOutOfRangeException(nameof(numberOfThreads), "Must be at least 1");
        _numberOfThreads = numberOfThreads;
        _threads = new Thread[16];
        for (int i = 0; i < 16; i++)
        {
            _threads[i] = new Thread(Run);
            _indexes.Add(_threads[i].ManagedThreadId, i);
        }
        ChangeNumberOfThreads(_numberOfThreads);
    }

    private void Run()
    {
        while (!_queue.IsCompleted && _indexes[Thread.CurrentThread.ManagedThreadId] < _numberOfThreads)
        {
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
                if (_threads[i].ThreadState != ThreadState.Running)
                {
                    _indexes.Remove(_threads[i].ManagedThreadId);
                    _threads[i] = new Thread(Run);
                    _indexes.Add(_threads[i].ManagedThreadId, i);
                    _threads[i].Start();
                }
            }
            Console.WriteLine($"Number of threads: {_numberOfThreads}");
        }
    }
}