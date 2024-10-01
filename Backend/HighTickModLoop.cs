using System;
using System.Threading.Tasks;
using FluentMigrator.Runner;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters;

public class HighTickModLoop(int framesPerSecond) : ModBase
{
    private StopWatch _stopWatch = new();
    private DateTime _lastTickTime;
    private ILogger<HighTickModLoop> _logger;

    public override async Task Start()
    {
        _stopWatch.Start();
        _lastTickTime = DateTime.UtcNow;
        _logger = ServiceProvider.CreateLogger<HighTickModLoop>();

        if (framesPerSecond <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(framesPerSecond), "Frames per second should be > 0");
        }

        while (true)
        {
            await OnTick();
        }
    }

    private async Task OnTick()
    {
        var currentTickTime = DateTime.UtcNow;
        var deltaTime = currentTickTime - _lastTickTime;
        _lastTickTime = currentTickTime;

        var fpsSeconds = 1d / framesPerSecond;
        if (deltaTime.TotalSeconds < fpsSeconds)
        {
            var waitSeconds = Math.Max(0, fpsSeconds - deltaTime.TotalSeconds);
            await Task.Delay(TimeSpan.FromSeconds(waitSeconds));
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