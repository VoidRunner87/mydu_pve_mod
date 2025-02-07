namespace Mod.DynamicEncounters.Features.Loot.Data;

public class SpawnFuelCommand(ulong constructId)
{
    public ulong ConstructId { get; } = constructId;
    public string FuelType = "Kergon1";
}