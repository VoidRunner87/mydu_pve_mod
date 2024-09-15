namespace Mod.DynamicEncounters.Features.Loot.Interfaces;

public class OreQuantity(long quantity) : IQuantity
{
    public long GetRawQuantity()
    {
        return quantity;
    }

    public long ToQuantity()
    {
        // Need to shift left the value
        return quantity << 24;
    }
}