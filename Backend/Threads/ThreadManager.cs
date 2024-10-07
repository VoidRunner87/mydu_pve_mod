using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Helpers;
using Timer = System.Timers.Timer;

namespace Mod.DynamicEncounters.Threads;

public class ThreadManager : IThreadManager
{
    private static ThreadManager? _instance;
    private readonly ConcurrentDictionary<ThreadId, CancellationTokenSource> _cancellationTokenSources = new();

    private readonly ConcurrentDictionary<ThreadId, DateTime> _heartbeatMap = new();
    private readonly ILogger<ThreadManager> _logger = ModBase.ServiceProvider.CreateLogger<ThreadManager>();
    private readonly ConcurrentDictionary<ThreadId, Thread> _threads = new();

    public static ThreadManager Instance
    {
        get { return _instance ??= new ThreadManager(); }
    }

    public void ReportHeartbeat(ThreadId threadId)
    {
        LoopStats.LastHeartbeatMap.AddOrUpdate(
            $"{threadId}",
            _ => DateTime.UtcNow,
            (_, _) => DateTime.UtcNow
        );

        _heartbeatMap.AddOrUpdate(
            threadId,
            _ => DateTime.UtcNow,
            (_, _) => DateTime.UtcNow
        );
    }

    public Task Start()
    {
        var taskCompletionSource = new TaskCompletionSource();

        var timer = new Timer(TimeSpan.FromSeconds(5));
        timer.Elapsed += (_, _) => { OnTimer(); };
        timer.Start();

        return taskCompletionSource.Task;
    }

    public void OnTimer()
    {
        var threadIds = Enum.GetValues<ThreadId>();

        foreach (var id in threadIds)
        {
            if (!DoesThreadExist(id))
            {
                var thread = CreateThread(id);
                RegisterThread(id, thread);
                thread.Start();

                continue;
            }

            if (IsThreadCancelled(id))
            {
                if (IsThreadStopped(id)) RemoveThread(id);
                continue;
            }

            if (DidThreadHang(id))
            {
                CancelThread(id);
                InterruptThread(id);
                RemoveThread(id);
            }
        }
    }

    public Dictionary<ThreadId, object> GetState()
    {
        var dict = _threads.ToDictionary(
            k => k.Key,
            v =>
            {
                _heartbeatMap.TryGetValue(v.Key, out var lastHeartbeat);
                _cancellationTokenSources.TryGetValue(v.Key, out var cts);

                return (object)new
                {
                    State = $"{v.Value.ThreadState}",
                    LastHeartbeat = lastHeartbeat,
                    IsThreadCancelled = cts?.IsCancellationRequested
                };
            });

        return dict;
    }

    private Thread CreateThread(ThreadId threadId)
    {
        _logger.LogInformation("Creating Thread {Thread}", threadId);

        var cts = CreateCancellationTokenSource(threadId);

        switch (threadId)
        {
            case ThreadId.Caching:
                return CreateThread(
                    threadId,
                    new CachingLoop(this, cts.Token).Tick
                );
            case ThreadId.Cleanup:
                return CreateThread(
                    threadId,
                    new CleanupLoop(this, cts.Token).Tick
                );
            case ThreadId.Sector:
                return CreateThread(
                    threadId,
                    new SectorLoop(this, cts.Token).Tick
                );
            case ThreadId.ExpirationNames:
                return CreateThread(
                    threadId,
                    new ExpirationNamesLoop(this, cts.Token).Tick
                );
            case ThreadId.TaskQueue:
                return CreateThread(
                    threadId,
                    new TaskQueueLoop(this, cts.Token).Tick
                );
            case ThreadId.BehaviorFeatureCheck:
                return CreateThread(
                    threadId,
                    new ConstructBehaviorFeatureCheckLoop(this, cts.Token).Tick
                );
            case ThreadId.ConstructHandleQuery:
                return CreateThread(
                    threadId,
                    new ConstructHandleListQueryLoop(this, cts.Token).Tick
                );
            case ThreadId.ConstructBehaviorMedium:
                return CreateThread(
                    threadId,
                    new ConstructBehaviorLoop(
                        ThreadId.ConstructBehaviorMedium,
                        this,
                        cts.Token,
                        1,
                        BehaviorTaskCategory.MediumPriority
                    ).Tick
                );
            case ThreadId.ConstructBehaviorHigh:
                return CreateThread(
                    threadId,
                    new ConstructBehaviorLoop(
                        ThreadId.ConstructBehaviorHigh,
                        this,
                        cts.Token,
                        10,
                        BehaviorTaskCategory.HighPriority
                    ).Tick
                );
            case ThreadId.ConstructBehaviorMovement:
                return CreateThread(
                    threadId,
                    new ConstructBehaviorLoop(
                        ThreadId.ConstructBehaviorMovement,
                        this,
                        cts.Token,
                        20,
                        BehaviorTaskCategory.MovementPriority
                    ).Tick
                );
            default:
                throw new ArgumentOutOfRangeException(nameof(threadId));
        }
    }

    public void CancelThread(ThreadId threadId)
    {
        _logger.LogInformation("Cancel Thread {Thread}", threadId);

        if (_cancellationTokenSources.TryGetValue(threadId, out var cts)) cts.Cancel();
    }

    private void RegisterThread(ThreadId threadId, Thread thread)
    {
        _logger.LogInformation("Registering Thread {Thread}", threadId);

        if (_threads.TryGetValue(threadId, out var oldThread)) oldThread.Interrupt();

        _threads.AddOrUpdate(
            threadId,
            _ => thread,
            (_, _) => thread
        );
    }

    public void InterruptThread(ThreadId threadId)
    {
        _logger.LogInformation("Interrupt Thread {Thread}", threadId);

        if (_threads.TryGetValue(threadId, out var thread)) thread.Interrupt();
    }

    private void RemoveThread(ThreadId threadId)
    {
        _logger.LogInformation("Remove Thread {Thread}", threadId);

        _threads.TryRemove(threadId, out _);
    }

    private Thread CreateThread(ThreadId threadId, Func<Task> action)
    {
        return new Thread(ThreadStart);

        async void ThreadStart()
        {
            await ThreadLoop(threadId, action);
        }
    }

    private async Task ThreadLoop(ThreadId threadId, Func<Task> action)
    {
        if (!_cancellationTokenSources.TryGetValue(threadId, out var cancellationTokenSource))
            throw new InvalidOperationException($"No Cancellation Token Source for ThreadId {threadId}");

        do
        {
            try
            {
                await action();
            }
            catch (Exception e)
            {
                ModBase.ServiceProvider.CreateLogger<ThreadManager>()
                    .LogError(e, "Thread {Id} Tick Failed", threadId);

                CancelThread(threadId);
            }
        } while (!cancellationTokenSource.Token.IsCancellationRequested);
    }

    private CancellationTokenSource CreateCancellationTokenSource(ThreadId threadId)
    {
        var source = new CancellationTokenSource();

        _cancellationTokenSources.AddOrUpdate(
            threadId,
            _ => source,
            (_, _) => source
        );

        return source;
    }

    private bool DidThreadHang(ThreadId threadId)
    {
        if (_heartbeatMap.TryGetValue(threadId, out var lastHeartbeat))
            return DateTime.UtcNow - lastHeartbeat > TimeSpan.FromMinutes(5);

        return false;
    }

    private bool IsThreadCancelled(ThreadId threadId)
    {
        if (!DoesThreadExist(threadId)) return false;

        if (!_cancellationTokenSources.TryGetValue(threadId, out var cts)) return true;

        return cts.IsCancellationRequested;
    }

    private bool IsThreadStopped(ThreadId threadId)
    {
        if (!_threads.TryGetValue(threadId, out var thread)) return true;

        return thread.ThreadState != ThreadState.Running;
    }

    private bool DoesThreadExist(ThreadId threadId)
    {
        return _threads.ContainsKey(threadId);
    }
}