using NpcCommonLib.Math;
using NpcHitCalculationLib.Data;

namespace NpcHitCalculationLib;

/// <summary>
/// Stateless, pure-function calculator for weapon hit/miss and stasis computations.
/// All methods are static with no side effects.
/// </summary>
/// <remarks>
/// <para>
/// Extracted from <c>WeaponGrainOverrides</c> (player/server-side weapon firing)
/// and <c>ShootWeaponAction</c> (NPC/client-side weapon firing). The original code
/// mixes these calculations with Orleans grain calls, scenegraph queries, and talent
/// lookups. This library contains only the pure math.
/// </para>
/// <para>
/// <b>Not handled by this class</b> (consumer responsibility):
/// <list type="bullet">
///   <item>Talent/buff resolution — caller applies <c>EffectSystem.ApplyModifiers</c> before passing values.</item>
///   <item>Damage calculation — depends on <c>EffectSystem</c> which is not a simple multiply.</item>
///   <item>Random number generation — caller provides RNG values where needed.</item>
///   <item>Hit/miss decision — caller compares <c>random.NextDouble()</c> against <see cref="HitCalculationOutput.HitRatio"/>.</item>
/// </list>
/// </para>
/// </remarks>
public static class HitCalculator
{
    /// <summary>
    /// Gaussian-style falloff factor used by all hit ratio sub-calculations.
    /// </summary>
    /// <param name="value">The measured value (angle, distance, angular velocity).</param>
    /// <param name="optimal">The optimal threshold below which the factor is 1.0.</param>
    /// <param name="falloff">Controls the rate of degradation beyond <paramref name="optimal"/>.</param>
    /// <param name="factor">Exponent scaling factor. Defaults to 1.0.</param>
    /// <returns>
    /// A value in (0, 1] where 1.0 means <paramref name="value"/> is at or below <paramref name="optimal"/>,
    /// decaying toward 0 as <paramref name="value"/> exceeds <paramref name="optimal"/>.
    /// </returns>
    /// <remarks>
    /// Formula: <c>0.5^(factor * ((max(0, value - optimal) / falloff)^2))</c>.
    /// When <paramref name="falloff"/> is zero or negative, returns 1.0 if value &lt;= optimal, else 0.0.
    /// </remarks>
    public static double ComputeFactor(double value, double optimal, double falloff, double factor = 1.0)
    {
        if (falloff <= 0.0)
            return value <= optimal ? 1.0 : 0.0;

        var delta = Math.Max(0.0, value - optimal) / falloff;
        return Math.Pow(0.5, factor * delta * delta);
    }

    /// <summary>
    /// Computes the hit ratio for a damaging weapon, combining accuracy, aiming cone,
    /// distance, tracking, and cross-section factors.
    /// </summary>
    /// <param name="input">All weapon, ammo, and positional parameters for the shot.</param>
    /// <returns>
    /// A <see cref="HitCalculationOutput"/> containing the final hit ratio and all intermediate
    /// factors. The hit ratio is not clamped — values above 1.0 guarantee a hit.
    /// </returns>
    public static HitCalculationOutput CalculateHitRatio(HitCalculationInput input)
    {
        var accuracy = input.BaseAccuracy * input.AmmoAccuracyModifier;

        var angleFactor = ComputeFactor(
            input.AngleDegrees, input.OptimalAimingCone, input.FalloffAimingCone);

        var distanceFactor = ComputeFactor(
            input.Distance, input.OptimalDistance, input.FalloffDistance);

        var trackingFactor = ComputeFactor(
            input.AngularVelocityDegrees, input.OptimalTracking, input.FalloffTracking);

        var optimalRadius = input.OptimalCrossSectionDiameter * 0.5;
        var optimalArea = optimalRadius * optimalRadius * Math.PI;

        var crossSectionFactor = optimalArea > 0.0
            ? Math.Min(1.0,
                Math.Sqrt(input.CrossSection / optimalArea) * (1.0 - trackingFactor) + trackingFactor)
            : 1.0;

        var hitRatio = accuracy * crossSectionFactor * angleFactor * distanceFactor * trackingFactor;

        return new HitCalculationOutput
        {
            HitRatio = hitRatio,
            Accuracy = accuracy,
            AngleFactor = angleFactor,
            DistanceFactor = distanceFactor,
            TrackingFactor = trackingFactor,
            CrossSectionFactor = crossSectionFactor,
        };
    }

    /// <summary>
    /// Computes the angular velocity of a target as seen from the shooter, in radians per second.
    /// </summary>
    /// <param name="shooterToTarget">Vector from shooter to target position.</param>
    /// <param name="relativeVelocity">Velocity of target minus velocity of shooter.</param>
    /// <param name="localAngularVelocity">Angular velocity of the shooter's construct.
    /// NPC callers pass <see cref="Vec3.Zero"/>.</param>
    /// <returns>The magnitude of the angular velocity vector in radians per second.</returns>
    /// <remarks>
    /// Formula: <c>|(shooterToTarget × relativeVelocity) / |shooterToTarget|² - localAngularVelocity|</c>.
    /// Returns 0 when the shooter-to-target distance is near-zero.
    /// </remarks>
    public static double ComputeAngularVelocity(
        Vec3 shooterToTarget,
        Vec3 relativeVelocity,
        Vec3 localAngularVelocity)
    {
        var distSq = shooterToTarget.LengthSquared();
        if (distSq < 1e-12)
            return 0.0;

        var angVel = shooterToTarget.CrossProduct(relativeVelocity) / distSq - localAngularVelocity;
        return angVel.Size();
    }

    /// <summary>
    /// Computes the effective range of a stasis weapon against a target of a given mass.
    /// </summary>
    /// <param name="input">Stasis weapon range parameters and target mass.</param>
    /// <returns>Effective range in metres.</returns>
    /// <remarks>
    /// Lighter constructs are affected at longer ranges. When <c>targetMass &gt; heavyConstructMass</c>,
    /// range falls back to <see cref="StasisRangeInput.RangeMax"/>.
    /// </remarks>
    public static double CalculateStasisRange(StasisRangeInput input)
    {
        if (input.TargetMass > input.HeavyConstructMass)
            return input.RangeMax;

        var num = (input.RangeMin - input.RangeMax) /
                  (1.0 - 1.0 / (input.RangeCurvature + 1.0));

        return input.RangeMin - num +
               num / (input.RangeCurvature * input.TargetMass / input.HeavyConstructMass + 1.0);
    }

    /// <summary>
    /// Determines whether a stasis weapon hits and computes the effect strength with distance falloff.
    /// </summary>
    /// <param name="input">Distance, range, and base effect strength.</param>
    /// <returns>Hit result and effective stasis strength.</returns>
    /// <remarks>
    /// Hit condition: <c>distance &lt;= range * 3</c>.
    /// Effect strength: <c>0.5^(max(distance - range, 0) / range) * baseEffectStrength</c>.
    /// </remarks>
    public static StasisHitOutput CalculateStasisHit(StasisHitInput input)
    {
        var isHit = input.Distance <= input.Range * 3.0;
        var overshoot = Math.Max(input.Distance - input.Range, 0.0);

        var effectStrength = input.Range > 0.0
            ? Math.Pow(0.5, overshoot / input.Range) * input.BaseEffectStrength
            : 0.0;

        return new StasisHitOutput
        {
            IsHit = isHit,
            EffectStrength = effectStrength,
        };
    }

    /// <summary>
    /// Computes a miss impact position by offsetting from the target along perpendicular
    /// and along-axis directions. The caller provides all random values for determinism.
    /// </summary>
    /// <param name="input">Origin, target, size, miss range, and three random values.</param>
    /// <returns>The world-space impact position of the missed shot.</returns>
    /// <remarks>
    /// The miss position is computed by:
    /// <list type="number">
    ///   <item>Computing an orthogonal basis around the shot direction.</item>
    ///   <item>Applying a random perpendicular offset (scaled by size and missRange).</item>
    ///   <item>Applying a random along-axis offset (scaled by size and missRange).</item>
    /// </list>
    /// </remarks>
    public static MissImpactOutput CalculateMissImpact(MissImpactInput input)
    {
        var direction = input.Target - input.Origin;
        var perpBasis1 = direction.Orthogonal();
        var perpBasis2 = direction.CrossProduct(perpBasis1).Normalized();

        var angle = input.RandomAngle * 2.0 * Math.PI;
        var perpDir = (Math.Cos(angle) * perpBasis1 + Math.Sin(angle) * perpBasis2).Normalized();
        var perpOffset = input.Size * (0.5 + input.RandomPerpMagnitude * input.MissRange) * perpDir;
        var alongOffset = input.Size * (0.5 + input.RandomAlongMagnitude * input.MissRange) * direction.Normalized();

        return new MissImpactOutput
        {
            ImpactPosition = input.Origin + direction + perpOffset + alongOffset,
        };
    }
}
