// See https://aka.ms/new-console-template for more information

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;

var tcs1 = new TaskCompletionSource();
Task.Run(async () =>
{
    await Task.Delay(10000);
    tcs1.SetResult();
});

tcs1.Task.Wait();
Console.ReadLine();

//var e = channel.Reader.ReadAllAsync().GetAsyncEnumerator();
//await e.MoveNextAsync();
//await e.MoveNextAsync();

var process = Process.Start("calc");
await process.WaitForExitAsync();



var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

Task.Run(async () =>
{
    tcs.SetResult();
});

Console.ReadLine();

var cq = new ConcurrentQueue<int>();


using (var facade = new DatabaseFacade())
using (var logger = new Logger(facade))
{
    logger.WriteLine("My message");
    await Task.Delay(100);
 
    await facade.SaveAsync("Another string");
    Console.WriteLine("The string is saved");
}


public static class WaitHandleExtensions
{
    public static Task WaitAsync(this WaitHandle handle)
    {
        var tcs = new TaskCompletionSource();
        ThreadPool.RegisterWaitForSingleObject(handle, (o, _) =>
        {
            var t = (TaskCompletionSource)o!;
            t.SetResult();
        }, tcs, 0, true);
        return tcs.Task;
    }
}

public class Logger : IDisposable
{
    private readonly DatabaseFacade _facade;
    private readonly BlockingCollection<string> _queue = new();
    private readonly Task _saveMessageTask;
 
    public Logger(DatabaseFacade facade) =>
        (_facade, _saveMessageTask) = (facade, Task.Run(SaveMessage));
 
    public void Dispose() => _queue.CompleteAdding();
 
    public void WriteLine(string message) => _queue.Add(message);
 
    private async Task SaveMessage()
    {
        foreach (var message in _queue.GetConsumingEnumerable())
        {
            // "Saving" message to the file
            Console.WriteLine($"Logger: {message}");
 
            // And to our database through the facade
            await _facade.SaveAsync(message);
        }
    }
}

public class DatabaseFacade : IDisposable
{
    private readonly BlockingCollection<(string item, TaskCompletionSource<string> result)> _queue =
        new ();

    private readonly Task _processItemsTask;

    public DatabaseFacade() => _processItemsTask = Task.Run(ProcessItems);

    public void Dispose() => _queue.CompleteAdding();

    public Task SaveAsync(string command)
    {
        var tcs = new TaskCompletionSource<string>();
        _queue.Add((item: command, result: tcs));
        return tcs.Task;
    }

    private async Task ProcessItems()
    {
        foreach (var item in _queue.GetConsumingEnumerable())
        {
            Console.WriteLine($"DatabaseFacade: executing '{item.item}'...");

            // Waiting a bit to emulate some IO-bound operation
            await Task.Delay(100);
            item.result.SetResult("OK");
            Console.WriteLine("DatabaseFacade: done.");
        }
    }
}