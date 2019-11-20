# Sharpnado.TaskMonitor

![task-monitor](https://raw.githubusercontent.com/roubachof/Sharpnado.TaskMonitor/master/Docs/task-monitor.png)

TaskMonitor is a task wrapper helping you to deal with "fire and forget" Task (non awaited Task) in a async/await environment.

It offers:

* Safe execution of all your async tasks: taylor-made for `async void` scenarios and non awaited Task
* Callbacks for any states (canceled, success, completed, failure)
* Default or custom error handler
* Default or custom task statistics logger

## Free yourself from async void!

Now let's say you have an evil async void in your code:

```csharp
public async void InitializeAsync()
{
    try
    {
        await InitializeMonkeyAsync();
    }
    catch (Exception exception)
    {
        // handle error
    }
}

private async Task InitializeMonkeyAsync()
{
    Monkey = await _monkeyService.GetMonkeyAsync();
}
```

You can get rid of async void and simply use the `TaskMonitor`:

```csharp
public void InitializeAsync()
{
    TaskMonitor.Create(InitializeMonkeyAsync);
}
```

If an error occurs, it will call the default error handler which will `Trace` the exception, so that it won't crash (async void) or fail silently (non awaited task).

```
TaskMonitor|ERROR|013|Error in wrapped task
Exception:System.Exception: Fault
   at Sharpnado.Tasks.Tests.TaskMonitorTest.DelayFaultAsync() in D:\Dev\Sharpnado\src\TaskMonitor\Sharpnado.TaskMonitor.Tests\TaskMonitorTest.cs:line 243
   at Sharpnado.Tasks.TaskMonitorBase.MonitorTaskAsync(Task task) in D:\Dev\Sharpnado\src\TaskMonitor\Sharpnado.TaskMonitor\TaskMonitorBase.cs:line 186
```

But you can also setup globally your own error handler with the `TaskMonitorConfiguration` static class:

```csharp
TaskMonitorConfiguration.ErrorHandler = (message, exception) =>
    {
        // Do custom stuff for exception handling;
    };
```            

**Now careful:** if you are in a MVVM scenario, I strongly encourage you to read my [Free Yourself From IsBusy](https://www.sharpnado.com/taskloaderview-async-init-made-easy/) post. The `ViewModelLoader` or the `NotifyTask` would be better to handle the `ViewModel` loading states.

## Features summary

Delegates for all states of the ran task:

```csharp
// Here task is "hot", it runs as soon as Create is called
TaskMonitor.Create(
    () => DoSomethingAsync(cts.Token), 
    t => isCompleted = true, 
    t => isFaulted = true,
    t => isSuccessfullyCompleted = true);
```

Builder for more elegant construction and deferred execution:

```csharp
var monitor = new TaskMonitor.Builder(() => DoSomethingAsync(cts.Token))
    .WithWhenCanceled(t => isCanceled = true)
    .WithWhenFaulted(t => isFaulted = true)
    .WithWhenSuccessfullyCompleted(t => isSuccess = true)
    .Build();

// explicit task start
monitor.Start();
```

Support for task with result, `Task<T>`:

```csharp
var monitor = TaskMonitor<List<string>>.Create(
    DelayListAsync, 
    t => isCompleted = true, 
    t => isFaulted = true,
    (task, result) =>
    {
        isSuccessfullyCompleted = true;
        Assert.Equal(3, result.Count);
    });

private async Task<List<string>> DelayListAsync()
{
    await Task.Delay(200);
    return new List<string> {"1", "2", "3"};
}
```

Default handling of errors and statistics, and naming of the monitor:

```csharp
TaskMonitorConfiguration.LogStatistics = true;
TaskMonitor.Create(
    DelayFaultAsync,
    name: "NominalFaultTestTask");
```

Output:

```
TaskMonitor|ERROR|013|Error in wrapped task
Exception:System.Exception: Fault
   at Sharpnado.Tasks.Tests.TaskMonitorTest.DelayFaultAsync() in D:\Dev\Sharpnado\src\TaskMonitor\Sharpnado.TaskMonitor.Tests\TaskMonitorTest.cs:line 262
   at Sharpnado.Tasks.TaskMonitorBase.MonitorTaskAsync(Task task) in D:\Dev\Sharpnado\src\TaskMonitor\Sharpnado.TaskMonitor\TaskMonitorBase.cs:line 186
TaskMonitor|STATS|013|NominalFaultTestTask => Status: IsFaulted, Executed in 246,55870000000002 ms
```

Global configuration for statistics and errors handlers:

```csharp
TaskMonitorConfiguration.LogStatistics = true;
TaskMonitorConfiguration.StatisticsHandler = (taskMonitor, timeSpan) =>
{
    statsHandlerCalled = true;
    Assert.True(timeSpan.TotalMilliseconds > 0);
};

TaskMonitorConfiguration.ErrorHandler = (taskMonitor, message, exception) =>
{
    errorHandlerCalled = true;
};

```

Run the wrapped `Task` in a new `Task`:

```csharp
int threadId = Thread.CurrentThread.ManagedThreadId;
var monitor = TaskMonitor<int>.Create(
    DelayThreadIdAsync,
    inNewTask: true);

await monitor.TaskCompleted;

Assert.NotEqual(threadId, monitor.Result);
```

Await the task monitor without failures. Awaiting on the `TaskCompleted` property will never fail:

```csharp
// Will always succeed wether the task is canceled, successful or faulted
await monitor.TaskCompleted;
```

Consider globally or locally the cancel state as faulted to simplify your workflow:

```csharp
// Local configuration
var cts = new CancellationTokenSource();
bool isFaulted = false;
bool isCanceled = false;

var monitor = new TaskMonitor.Builder(() => DelayCanceledAsync(cts.Token))
    .WithWhenCanceled(t => isCanceled = true)
    .WithWhenFaulted(t => isFaulted = true)
    .WithConsiderCanceledAsFaulted(true)
    .Build();

cts.Cancel();
monitor.Start();

await monitor.TaskCompleted;

Assert.True(!isCanceled && monitor.IsCanceled);
Assert.True(isFaulted && monitor.IsFaulted);
```

```csharp
// Global configuration
var cts = new CancellationTokenSource();
TaskMonitorConfiguration.ConsiderCanceledAsFaulted = true;
bool isFaulted = false;
bool isCanceled = false;
bool isSuccess = false;

var monitor = new TaskMonitor.Builder(() => DelayCanceledAsync(cts.Token))
    .WithWhenCanceled(t => isCanceled = true)
    .WithWhenFaulted(t => isFaulted = true)
    .Build();

cts.Cancel();
monitor.Start();

await monitor.TaskCompleted;

Assert.True(!isCanceled && monitor.IsCanceled);
Assert.True(isFaulted && monitor.IsFaulted);
```

## Other common scenarios

### Good with events or messages

If you are subscribing to an event/message and want to make async stuff when the event is raised, it will also be a perfect candidate.

```csharp

public Constructor(IMonkeyService monkeyService)
{
    monkeyService.MonkeyChanged += OnMonkeyChanged;
    // same as messageService.Subscribe("MonkeyChangedMessage", OnMonkeyChanged)
}

private void OnMonkeyChanged(MonkeyChangedEventArgs eventArgs)
{
    TaskMonitor.Create(() => DoSomethingAsync(eventArgs.Monkey));
}

private Task DoSomethingAsync(Monkey monkey)
{
    await CrazyAsyncStuff(monkey);
    await SomeOtherCrazyAction();
}

```

### Good with non awaited task and ContinueWith

Previously you maybe used the `ContinueWith` task method to create a new task and deal with fire and forget scenarios.

```csharp
public void DoSomethingAsync()
{
    // Task will not be awaited and you are still handling the exception: hooray!
    Task.Run(() => InitializeMonkey())
        .ContinueWith(
            t => HandleException(t.InnerException),
            TaskContinuationOptions.OnlyOnFaulted);
}

private void InitializeMonkey()
{
    ...
}

```

You can achieve the same behaviour with the `TaskMonitor` in a cleaner way:

```csharp
public void DoSomethingAsync()
{
    TaskMonitor.Create(Task.Run(() => InitializeMonkey()));
}
```

### Can be used as a simple decorator for statistics and error handling

You can specify global error handler and statistics logger:

```csharp
TaskMonitorConfiguration.LogStatistics = true;
TaskMonitorConfiguration.StatisticsHandler = (taskMonitor, timeSpan) =>
{
    // My global statistics logger
};

TaskMonitorConfiguration.ErrorHandler = (taskMonitor, message, exception) =>
{
    // My global error logger
};
```

Then use `TaskMonitor` as a simple task logging decorator:

```csharp
try
{
    await TaskMonitor.Create(DelayFaultAsync, name: "UseMonitorAsDecoratedFaultTest").Task;
}
catch(Exception exception)
{
    // handle exception
}
```

But you can also let the default handlers `Trace` the errors and the statistics.
Output with default handlers:

```
TaskMonitor|ERROR|013|Error in wrapped task
Exception:System.Exception: Fault
   at Sharpnado.Tasks.Tests.TaskMonitorTest.DelayFaultAsync() in D:\Dev\Sharpnado\src\TaskMonitor\Sharpnado.TaskMonitor.Tests\TaskMonitorTest.cs:line 262
   at Xunit.Assert.RecordExceptionAsync(Func`1 testCode) in C:\Dev\xunit\xunit\src\xunit.assert\Asserts\Record.cs:line 82
TaskMonitor|STATS|013|UseMonitorAsDecoratedFaultTest => Status: IsFaulted, Executed in 334,27070000000003 ms
```


## Origins

The `TaskMonitor` was inspired by [Stephen Cleary](https://blog.stephencleary.com/)'s `NotifyTask`. It's a task wrapper dealing with non-awaited, or fire and forget if you prefer, Task.
The difference is that `NotifyTask` is made for UI scenarios (MVVM), where you want to bind the result or the state of the task to a view property. For this it implements `INotifyPropertyChanged`.
See this: https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming

Whereas `TaskMonitor` is designed to be used in any kind of scenarios (server, business layer, UI, etc...).