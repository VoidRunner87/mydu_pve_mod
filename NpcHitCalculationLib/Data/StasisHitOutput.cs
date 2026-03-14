namespace NpcHitCalculationLib.Data;

/// <summary>
/// Result of a stasis hit evaluation.
/// </summary>
public class StasisHitOutput
{
    /// <summary>Whether the stasis weapon hit the target (distance &lt;= range * 3).</summary>
    public required bool IsHit { get; init; }

    /// <summary>Effective strength of the stasis effect after distance falloff. Zero when missed.</summary>
    public required double EffectStrength { get; init; }
}
