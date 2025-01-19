using NQ;

namespace Mod.DynamicEncounters.Features.Common.Data;

public class ScanContact(string name, ulong constructId, double distance, Vec3 position)
{
    public ulong ConstructId { get; set; } = constructId;
    public string Name { get; set; } = name;
    public double Distance { get; set; } = distance;
    public Vec3 Position { get; set; } = position;
}