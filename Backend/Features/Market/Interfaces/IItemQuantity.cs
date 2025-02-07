namespace Mod.DynamicEncounters.Features.Market.Interfaces;

public interface IItemQuantity
{
    long Value { get; }

    double GetReadableValue();
}