using NQ;

namespace Mod.DynamicEncounters.Features.Common.Data;

public class PlayerConstructContact(string name, ulong constructId, double distance) : ScanContact(name, constructId, distance, new Vec3())
{
    public required string PlayerName { get; set; }
}