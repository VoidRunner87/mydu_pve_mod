using NQ;

namespace Mod.DynamicEncounters.Overrides.Common.Data;

public class PlayerPosition
{
    public bool Valid { get; set; }
    public ulong ConstructId { get; set; }
    public Vec3 Position { get; set; }
}