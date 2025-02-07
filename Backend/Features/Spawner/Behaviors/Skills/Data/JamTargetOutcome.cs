namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Data;

public class JamTargetOutcome
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public static JamTargetOutcome Jammed() => new() { Success = true };
    public static JamTargetOutcome FailedTargetWithoutPilot() => new() { Message = "Target without pilot" };
}