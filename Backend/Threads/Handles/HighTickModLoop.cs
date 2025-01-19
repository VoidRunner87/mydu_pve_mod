using System;
using System.Threading;
using System.Threading.Tasks;
using FluentMigrator.Runner;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Threads.Handles;

public abstract class HighTickModLoop : BackgroundService
{
    private StopWatch _stopWatch = new();
    private DateTime _lastTickTime;
    private readonly double _framesPerSecond;
    private readonly bool _fixedStep;
    private const double FixedDeltaTime = 1 / 20d;
    private const int MaxFixedStepLoops = 10;
    private TimeSpan _accumulatedTime = TimeSpan.Zero;

    protected HighTickModLoop(
        double framesPerSecond, 
        bool fixedStep
    )
    {
        _framesPerSecond = framesPerSecond;
        _fixedStep = fixedStep;
        _stopWatch.Start();
        _lastTickTime = DateTime.UtcNow;

        if (_framesPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(_framesPerSecond), "Frames per second should be > 0");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, timeoutCts.Token);
            
                await Tick(cts.Token);
                await Task.Yield();
            }
            catch (Exception e)
            {
                ModBase.ServiceProvider.CreateLogger<HighTickModLoop>()
                    .LogError(e, "{Type} Exception: {Message}", GetType().Name, e.Message);
            }
        }
    }

    private Task Tick(CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested) return Task.CompletedTask;
        
        if (_fixedStep)
        {
            return FixedStepTickInternal(stoppingToken);
        }

        return TickInternal(stoppingToken);
    }

    private async Task TickInternal(CancellationToken stoppingToken)
    {
        var currentTickTime = DateTime.UtcNow;
        var deltaTime = currentTickTime - _lastTickTime;
        _lastTickTime = currentTickTime;

        var fpsSeconds = 1d / _framesPerSecond;
        if (deltaTime.TotalSeconds < fpsSeconds)
        {
            var waitSeconds = Math.Max(0, fpsSeconds - deltaTime.TotalSeconds);
            await Task.Delay(TimeSpan.FromSeconds(waitSeconds), stoppingToken);
        }
            
        await Tick(deltaTime, stoppingToken);

        _stopWatch = new StopWatch();
        _stopWatch.Start();
    }

    private async Task FixedStepTickInternal(CancellationToken stoppingToken)
    {
        var currentTickTime = DateTime.UtcNow;
        var deltaTime = currentTickTime - _lastTickTime;
        _lastTickTime = currentTickTime;

        var fpsSeconds = 1d / _framesPerSecond;
        if (deltaTime.TotalSeconds < fpsSeconds)
        {
            var waitSeconds = Math.Max(0, fpsSeconds - deltaTime.TotalSeconds);
            await Task.Delay(TimeSpan.FromSeconds(waitSeconds), stoppingToken);
        }

        _accumulatedTime += deltaTime;
        var fixedDeltaSpan = TimeSpan.FromSeconds(FixedDeltaTime);

        var tickCount = 0;
        while (_accumulatedTime >= fixedDeltaSpan)
        {
            if (stoppingToken.IsCancellationRequested) return;
            
            await Tick(fixedDeltaSpan, stoppingToken);
            _accumulatedTime -= fixedDeltaSpan;

            tickCount++;

            if (tickCount > MaxFixedStepLoops)
            {
                _accumulatedTime = TimeSpan.Zero;
                break;
            }
        }

        _stopWatch = new StopWatch();
        _stopWatch.Start();
    }

    public virtual Task Tick(TimeSpan deltaTime, CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}