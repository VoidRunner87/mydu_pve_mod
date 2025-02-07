using Mod.DynamicEncounters.Features.Market.Interfaces;

namespace Mod.DynamicEncounters.Features.Market.Data;

public class Quantity(long value) : IItemQuantity
{
    public long Value { get; } = value;
    
    public double GetReadableValue() => Value;
    
    public override string ToString()
    {
        return $"{Value:N2}";
    }
}