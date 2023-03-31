using System.Collections.Concurrent;

namespace HW4;

public class CsvFileThreadTaskScheduler : TaskScheduler, IDisposable
{
    private readonly BlockingCollection<Task> _queue = new ();

    private int _numberOfThreads;

    private List<Thread> _threads;


    public CsvFileThreadTaskScheduler(int numberOfThreads)
    {
        if (numberOfThreads < 1)
            throw new ArgumentOutOfRangeException(nameof(numberOfThreads), "Must be at least 1");
        _numberOfThreads = numberOfThreads;
        _threads = new List<Thread>(_numberOfThreads);
        for (int i = 0; i < _numberOfThreads; i++)
        {
            _threads.Add(new Thread(Run));
            _threads[i].Start();
        }
        Console.WriteLine($"Number of threads: {_threads.Count}");
    }

    private void Run()
    {
        while (!_queue.IsCompleted)
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

        foreach (var thread in _threads)
        { 
            if (thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
                thread.Join();
        }
    }
}