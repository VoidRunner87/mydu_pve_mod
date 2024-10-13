namespace Mod.DynamicEncounters.Features.Common.Data;

public class NpcRadarContact(string name, ulong constructId)
{
    public ulong ConstructId { get; set; } = constructId;
    public string Name { get; set; } = name;
}