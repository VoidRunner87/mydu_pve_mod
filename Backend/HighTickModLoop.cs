using System;
using System.Threading;
using System.Threading.Tasks;
using FluentMigrator.Runner;
using Mod.DynamicEncounters.Threads;

namespace Mod.DynamicEncounters;

public abstract class HighTickModLoop : ThreadHandle
{
    private StopWatch _stopWatch = new();
    private DateTime _lastTickTime;
    private readonly int _framesPerSecond;

    protected HighTickModLoop(
        int framesPerSecond, 
        ThreadId threadId,
        IThreadManager threadManager,
        CancellationToken token
    ) : base(threadId, threadManager, token)
    {
        _framesPerSecond = framesPerSecond;
        _stopWatch.Start();
        _lastTickTime = DateTime.UtcNow;

        if (_framesPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(_framesPerSecond), "Frames per second should be > 0");
        }
    }

    public override async Task Tick()
    {
        var currentTickTime = DateTime.UtcNow;
        var deltaTime = currentTickTime - _lastTickTime;
        _lastTickTime = currentTickTime;

        var fpsSeconds = 1d / _framesPerSecond;
        if (deltaTime.TotalSeconds < fpsSeconds)
        {
            var waitSeconds = Math.Max(0, fpsSeconds - deltaTime.TotalSeconds);
            Thread.Sleep(TimeSpan.FromSeconds(waitSeconds));
        }
            
        await Tick(deltaTime);

        _stopWatch = new StopWatch();
        _stopWatch.Start();
    }

    public virtual Task Tick(TimeSpan deltaTime)
    {
        return Task.CompletedTask;
    }
}