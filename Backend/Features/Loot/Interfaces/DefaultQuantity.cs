namespace Mod.DynamicEncounters.Features.Loot.Interfaces;

public class DefaultQuantity(long quantity) : IQuantity
{
    public long ToQuantity()
    {
        return quantity;
    }
}