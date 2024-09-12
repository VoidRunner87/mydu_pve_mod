namespace Mod.DynamicEncounters.Features.Loot.Data;

public class SpawnItemCommand(ulong constructId, ItemBagData itemBag)
{
    public ulong ConstructId { get; } = constructId;
    public ItemBagData ItemBag { get; } = itemBag;
}