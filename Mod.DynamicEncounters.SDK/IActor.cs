namespace Mod.DynamicEncounters.SDK;

public interface IActor
{
    double FramesPerSecond { get; }
    bool FixedStep { get; }

    Task StartAsync(CancellationToken cancellationToken);
    Task Tick(TimeSpan deltaTime, CancellationToken stoppingToken);
    Task StopAsync(CancellationToken cancellationToken);
}