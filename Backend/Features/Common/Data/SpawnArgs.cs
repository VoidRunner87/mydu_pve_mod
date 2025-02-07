using NQ;

namespace Mod.DynamicEncounters.Features.Common.Data;

public class SpawnArgs
{
    public string File { get; set; }
    public string Folder { get; set; }
    public Vec3 Position { get; set; }
    public string Name { get; set; }
    public EntityId OwnerEntityId { get; set; }
    public bool IsUntargetable { get; set; }
    public bool IsNpc { get; set; }
    public bool IsDynamicWreck { get; set; }
}