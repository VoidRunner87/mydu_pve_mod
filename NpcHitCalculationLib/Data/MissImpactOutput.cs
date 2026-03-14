using NpcCommonLib.Math;

namespace NpcHitCalculationLib.Data;

/// <summary>
/// Result of a miss impact position calculation.
/// </summary>
public class MissImpactOutput
{
    /// <summary>World-space position where the missed shot impacts.</summary>
    public required Vec3 ImpactPosition { get; init; }
}
