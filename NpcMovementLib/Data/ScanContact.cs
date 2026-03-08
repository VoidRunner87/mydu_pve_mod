using NpcMovementLib.Math;

namespace NpcMovementLib.Data;

public class ScanContact
{
    public required ulong ConstructId { get; set; }
    public required string Name { get; set; }
    public required double Distance { get; set; }
    public required Vec3 Position { get; set; }
}
