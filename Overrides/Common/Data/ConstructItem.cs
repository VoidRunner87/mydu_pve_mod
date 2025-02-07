using NQ;

namespace Mod.DynamicEncounters.Overrides.Common.Data;

public class ConstructItem
{
    public ulong Id { get; set; }
    public string Name { get; set; }
    public long Size { get; set; }
    public ConstructKind Kind { get; set; }
    public double ShieldRatio { get; set; }
}