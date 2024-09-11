namespace Mod.DynamicEncounters.Features.Loot.Data;

public class SpawnItemCommand(ulong constructId, ItemData item)
{
    public ulong ConstructId { get; } = constructId;
    public ItemData Item { get; } = item;
}