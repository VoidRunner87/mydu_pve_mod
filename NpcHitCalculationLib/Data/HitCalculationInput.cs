namespace NpcHitCalculationLib.Data;

/// <summary>
/// Input parameters for computing the hit ratio of a damaging weapon shot.
/// </summary>
/// <remarks>
/// Extracted from <c>WeaponGrainOverrides.CalculateHitRatio</c> (player/server-side)
/// and <c>ShootWeaponAction.CalculateHitRatio</c> (NPC/client-side).
/// NPC callers pass <see cref="AngleDegrees"/> = 0 (perfect aim) and
/// <see cref="AngularVelocityDegrees"/> = 0 (no construct rotation tracking).
/// </remarks>
public class HitCalculationInput
{
    /// <summary>Base accuracy of the weapon (0..1), with weapon modifiers already applied.</summary>
    public required double BaseAccuracy { get; set; }

    /// <summary>Ammo accuracy modifier (multiplier, typically 1.0).</summary>
    public required double AmmoAccuracyModifier { get; set; }

    /// <summary>Angle between weapon bore-sight and target direction, in degrees. NPC callers pass 0.</summary>
    public required double AngleDegrees { get; set; }

    /// <summary>Optimal aiming cone half-angle in degrees.</summary>
    public required double OptimalAimingCone { get; set; }

    /// <summary>Falloff aiming cone in degrees.</summary>
    public required double FalloffAimingCone { get; set; }

    /// <summary>Distance from weapon to target in metres.</summary>
    public required double Distance { get; set; }

    /// <summary>Optimal engagement distance in metres.</summary>
    public required double OptimalDistance { get; set; }

    /// <summary>Falloff distance in metres.</summary>
    public required double FalloffDistance { get; set; }

    /// <summary>Angular velocity of the target relative to the shooter, in degrees per second.</summary>
    public required double AngularVelocityDegrees { get; set; }

    /// <summary>Optimal tracking speed in degrees per second.</summary>
    public required double OptimalTracking { get; set; }

    /// <summary>Falloff tracking speed in degrees per second.</summary>
    public required double FalloffTracking { get; set; }

    /// <summary>Cross-section area of the target.</summary>
    public required double CrossSection { get; set; }

    /// <summary>Optimal cross-section diameter of the weapon in metres. Halved internally for radius.</summary>
    public required double OptimalCrossSectionDiameter { get; set; }
}
