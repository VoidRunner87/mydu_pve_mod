namespace Mod.DynamicEncounters.Features.Loot.Interfaces;

public interface IQuantity
{
    long GetRawQuantity();
    long ToQuantity();
}