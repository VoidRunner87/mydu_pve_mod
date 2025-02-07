namespace Mod.DynamicEncounters.Features.Loot.Interfaces;

public class LitreQuantity(long quantity) : IQuantity
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
    
    public override string ToString()
    {
        return $"Raw: {GetRawQuantity()} | {ToQuantity()}";
    }
}