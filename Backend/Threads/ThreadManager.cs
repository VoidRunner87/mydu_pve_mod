using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Helpers;
using ThreadState = System.Threading.ThreadState;

namespace Mod.DynamicEncounters.Threads;

public class ThreadManager : BackgroundService, IThreadManager
{
    private static ThreadManager? _instance;
    private readonly ConcurrentDictionary<ThreadId, CancellationTokenSource> _cancellationTokenSources = new();
    private readonly ConcurrentDictionary<ThreadId, DateTime> _heartbeatMap = new();
    private readonly ILogger<ThreadManager> _logger = ModBase.ServiceProvider.CreateLogger<ThreadManager>();
    private readonly ConcurrentDictionary<ThreadId, Thread> _threads = new();
    private readonly ConcurrentDictionary<ThreadId, bool> _threadBlocks = new();
    private readonly ConcurrentDictionary<ThreadId, DateTime> _threadStartMap = new();

    public static ThreadManager GetInstance()
    {
        return _instance ??= new ThreadManager();
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

    public void CancelAllThreads()
    {
        var threadIds = Enum.GetValues<ThreadId>();

        foreach (var id in threadIds)
        {
            CancelThread(id);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            OnTick(stoppingToken);
            await Task.Delay(1000, stoppingToken);
        }
    }

    public void OnTick(CancellationToken stoppingToken)
    {
        var sw = new Stopwatch();
        sw.Start();
        var threadIds = Enum.GetValues<ThreadId>();

        foreach (var id in threadIds)
        {
            if (stoppingToken.IsCancellationRequested) break;
            
            if (IsCreationBlocked(id)) continue;
            
            if (!DoesThreadExist(id))
            {
                var thread = CreateThread(id);
                RegisterThread(id, thread);
                thread.Start();

                continue;
            }

            if (IsThreadOld(id) && !IsThreadCancelled(id))
            {
                _logger.LogWarning("Thread {ThreadId} is old", id);
                CancelThread(id);
                continue;
            }

            if (IsThreadCancelled(id))
            {
                if (IsThreadStopped(id)) RemoveThread(id);
                continue;
            }

            if (DidThreadHang(id))
            {
                _logger.LogWarning("Thread {ThreadId} Hang", id);
                
                if (!IsThreadCancelled(id))
                {
                    CancelThread(id);
                }
                // else
                // {
                //     InterruptThread(id);
                //     RemoveThread(id);
                // }
            }
        }

        StatsRecorder.Record(nameof(ThreadManager), sw.ElapsedMilliseconds);
    }

    public Dictionary<ThreadId, object> GetState()
    {
        var dict = _threads.ToDictionary(
            k => k.Key,
            v =>
            {
                _heartbeatMap.TryGetValue(v.Key, out var lastHeartbeat);
                _cancellationTokenSources.TryGetValue(v.Key, out var cts);
                _threadStartMap.TryGetValue(v.Key, out var startDate);

                return (object)new
                {
                    State = $"{v.Value.ThreadState}",
                    LastHeartbeat = lastHeartbeat,
                    IsThreadCancelled = cts?.IsCancellationRequested,
                    StartDate = startDate
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
            default:
                throw new ArgumentOutOfRangeException(nameof(threadId));
        }
    }

    public bool IsCreationBlocked(ThreadId threadId) 
        => _threadBlocks.TryGetValue(threadId, out var blocked) && blocked;

    public void BlockThreadCreation(ThreadId threadId)
    {
        _threadBlocks.TryAdd(threadId, true);
    }
    
    public void UnblockThreadCreation(ThreadId threadId)
    {
        _threadBlocks.TryRemove(threadId, out _);
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

        _threadStartMap.AddOrUpdate(
            threadId,
            _ => DateTime.UtcNow,
            (_, _) => DateTime.UtcNow
        );

        _threads.AddOrUpdate(
            threadId,
            _ => thread,
            (_, _) => thread
        );
        
        _heartbeatMap.AddOrUpdate(
            threadId,
            _ => DateTime.UtcNow,
            (_, _) => DateTime.UtcNow
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

        _threadStartMap.TryRemove(threadId, out _);
        _threads.TryRemove(threadId, out _);
    }

    private Thread CreateThread(ThreadId threadId, Func<Task> action)
    {
        return new Thread(Run);

        void Run()
        {
            ThreadLoop(threadId, action).Wait();
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
            return DateTime.UtcNow - lastHeartbeat > TimeSpan.FromMinutes(2);

        return false;
    }

    private bool IsThreadOld(ThreadId threadId)
    {
        if (!DoesThreadExist(threadId)) return false;

        if (!_threadStartMap.TryGetValue(threadId, out var threadStartDate)) return false;

        var timeSpan = DateTime.UtcNow - threadStartDate;
        return timeSpan  > TimeSpan.FromHours(3);
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