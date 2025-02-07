namespace Mod.DynamicEncounters.Features.Warp.Data;

public class SetWarpCooldownOutcome
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public static SetWarpCooldownOutcome NotASupercruiseDrive(string elementTypeName)
    {
        return new SetWarpCooldownOutcome { Message = $"Element is not a Super Cruise drive {elementTypeName}" };
    }
    
    public static SetWarpCooldownOutcome InvalidConstruct()
    {
        return new SetWarpCooldownOutcome { Message = "Invalid Construct" };
    }

    public static SetWarpCooldownOutcome CooldownSet()
    {
        return new SetWarpCooldownOutcome { Success = true };
    }
}