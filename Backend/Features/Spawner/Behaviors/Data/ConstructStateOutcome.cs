namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Data;

public class ConstructStateOutcome
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ConstructStateItem? StateItem { get; set; }

    public static ConstructStateOutcome Added() => new() { Success = true, Message = "Added" };

    public static ConstructStateOutcome Updated() => new() { Success = true, Message = "Updated" };
    public static ConstructStateOutcome NotFound(string type, ulong constructId) => new() { Message = $"{constructId}/{type} Not Found" };

    public static ConstructStateOutcome Retrieved(ConstructStateItem item) =>
        new() { StateItem = item, Success = true };
}