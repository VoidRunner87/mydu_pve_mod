namespace NpcHitCalculationLib.Data;

/// <summary>
/// Input parameters for computing the effective range of a stasis weapon against a target.
/// </summary>
/// <remarks>
/// Range varies by target mass: lighter constructs are affected at longer ranges.
/// Extracted from <c>WeaponGrainOverrides.WeaponFireStasis</c>.
/// </remarks>
public class StasisRangeInput
{
    /// <summary>Total mass of the target construct.</summary>
    public required double TargetMass { get; set; }

    /// <summary>Mass threshold defining a heavy construct (from <c>ConstructSpeedConfig</c>).</summary>
    public required double HeavyConstructMass { get; set; }

    /// <summary>Minimum range of the stasis weapon (effective against lightest constructs).</summary>
    public required double RangeMin { get; set; }

    /// <summary>Maximum range of the stasis weapon (effective against heaviest constructs).</summary>
    public required double RangeMax { get; set; }

    /// <summary>Curvature parameter controlling the range interpolation curve.</summary>
    public required double RangeCurvature { get; set; }
}
