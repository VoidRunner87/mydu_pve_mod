using NpcCommonLib.Math;

namespace NpcHitCalculationLib.Data;

/// <summary>
/// Input parameters for computing a miss impact position.
/// Random values are provided by the caller to keep this library stateless and deterministic.
/// </summary>
/// <remarks>
/// Extracted from <c>WeaponGrainOverrides.CalculateMissImpact</c> and
/// <c>ShootWeaponAction.CalculateMissImpact</c>.
/// </remarks>
public class MissImpactInput
{
    /// <summary>Origin position of the shot (shooter world location).</summary>
    public required Vec3 Origin { get; set; }

    /// <summary>Target position (intended hit point, world location).</summary>
    public required Vec3 Target { get; set; }

    /// <summary>Size of the target bounding volume (used to scale the miss offset).</summary>
    public required double Size { get; set; }

    /// <summary>Miss range multiplier controlling how far the miss deviates.</summary>
    public required double MissRange { get; set; }

    /// <summary>Random value in [0, 1) used for the angular offset around the shot direction.</summary>
    public required double RandomAngle { get; set; }

    /// <summary>Random value in [0, 1) used for the perpendicular offset magnitude.</summary>
    public required double RandomPerpMagnitude { get; set; }

    /// <summary>Random value in [0, 1) used for the along-axis offset magnitude.</summary>
    public required double RandomAlongMagnitude { get; set; }
}
