namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Data;

public class JamConstructCommand
{
    public ulong InstigatorConstructId { get; set; }
    public ulong TargetConstructId { get; set; }
    public double DurationSeconds { get; set; } = 1;
    public bool SendAlert { get; set; } = true;
}