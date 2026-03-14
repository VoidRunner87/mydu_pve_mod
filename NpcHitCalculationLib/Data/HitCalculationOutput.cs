namespace NpcHitCalculationLib.Data;

/// <summary>
/// Result of a hit ratio calculation, including the final ratio and all intermediate factors.
/// </summary>
/// <remarks>
/// The <see cref="HitRatio"/> is not clamped — values above 1.0 indicate a guaranteed hit
/// (the random threshold <c>[0,1)</c> is always exceeded). This matches the original game behaviour.
/// </remarks>
public class HitCalculationOutput
{
    /// <summary>Final hit ratio. Values above 1.0 guarantee a hit.</summary>
    public required double HitRatio { get; init; }

    /// <summary>Combined accuracy (baseAccuracy * ammoAccuracyModifier).</summary>
    public required double Accuracy { get; init; }

    /// <summary>Aiming cone factor from ComputeFactor (1.0 when within optimal cone).</summary>
    public required double AngleFactor { get; init; }

    /// <summary>Distance factor from ComputeFactor (1.0 when within optimal distance).</summary>
    public required double DistanceFactor { get; init; }

    /// <summary>Tracking factor from ComputeFactor (1.0 when angular velocity is within optimal tracking).</summary>
    public required double TrackingFactor { get; init; }

    /// <summary>Cross-section factor (1.0 for targets at or above optimal cross-section).</summary>
    public required double CrossSectionFactor { get; init; }
}
