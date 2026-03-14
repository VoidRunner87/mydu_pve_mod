using NpcCommonLib.Math;
using NpcHitCalculationLib;
using NpcHitCalculationLib.Data;
using Xunit;
using Xunit.Abstractions;

namespace NpcHitCalculationLib.Tests;

public class HitCalculatorTests
{
    private readonly ITestOutputHelper _output;

    public HitCalculatorTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // ── ComputeFactor ──────────────────────────────────────────────

    [Fact]
    public void ComputeFactor_AtOptimal_ReturnsOne()
    {
        var result = HitCalculator.ComputeFactor(10.0, 10.0, 5.0);
        Assert.Equal(1.0, result);
    }

    [Fact]
    public void ComputeFactor_BelowOptimal_ReturnsOne()
    {
        var result = HitCalculator.ComputeFactor(5.0, 10.0, 5.0);
        Assert.Equal(1.0, result);
    }

    [Fact]
    public void ComputeFactor_AtOneFalloff_ReturnsHalf()
    {
        // 0.5^(1 * ((15-10)/5)^2) = 0.5^(1*1) = 0.5
        var result = HitCalculator.ComputeFactor(15.0, 10.0, 5.0);
        Assert.Equal(0.5, result, precision: 10);
    }

    [Fact]
    public void ComputeFactor_AtTwoFalloffs_ReturnsQuarter()
    {
        // 0.5^(1 * ((20-10)/5)^2) = 0.5^4 = 0.0625
        var result = HitCalculator.ComputeFactor(20.0, 10.0, 5.0);
        Assert.Equal(0.0625, result, precision: 10);
    }

    [Fact]
    public void ComputeFactor_ZeroFalloff_ReturnsStepFunction()
    {
        Assert.Equal(1.0, HitCalculator.ComputeFactor(5.0, 10.0, 0.0));
        Assert.Equal(1.0, HitCalculator.ComputeFactor(10.0, 10.0, 0.0));
        Assert.Equal(0.0, HitCalculator.ComputeFactor(10.1, 10.0, 0.0));
    }

    [Fact]
    public void ComputeFactor_WithFactorParam_ScalesExponent()
    {
        // 0.5^(2 * ((15-10)/5)^2) = 0.5^2 = 0.25
        var result = HitCalculator.ComputeFactor(15.0, 10.0, 5.0, 2.0);
        Assert.Equal(0.25, result, precision: 10);
    }

    // ── CalculateHitRatio ──────────────────────────────────────────

    [Fact]
    public void CalculateHitRatio_AtOptimalRange_PerfectAim_ReturnsHighRatio()
    {
        var input = new HitCalculationInput
        {
            BaseAccuracy = 0.9,
            AmmoAccuracyModifier = 1.0,
            AngleDegrees = 0.0,          // perfect aim
            OptimalAimingCone = 3.0,
            FalloffAimingCone = 1.5,
            Distance = 50_000,            // at optimal
            OptimalDistance = 50_000,
            FalloffDistance = 20_000,
            AngularVelocityDegrees = 0.0, // stationary target
            OptimalTracking = 1.0,
            FalloffTracking = 0.5,
            CrossSection = 5.0,
            OptimalCrossSectionDiameter = 32.0,
        };

        var output = HitCalculator.CalculateHitRatio(input);

        _output.WriteLine($"HitRatio={output.HitRatio:F4} Accuracy={output.Accuracy:F4} " +
                          $"Angle={output.AngleFactor:F4} Distance={output.DistanceFactor:F4} " +
                          $"Tracking={output.TrackingFactor:F4} CrossSection={output.CrossSectionFactor:F4}");

        Assert.Equal(1.0, output.AngleFactor);     // within cone
        Assert.Equal(1.0, output.DistanceFactor);   // at optimal
        Assert.Equal(1.0, output.TrackingFactor);    // stationary
        Assert.True(output.HitRatio > 0.0);
    }

    [Fact]
    public void CalculateHitRatio_BeyondOptimalDistance_ReducesRatio()
    {
        var atOptimal = CreateDefaultInput(distance: 50_000);
        var beyondOptimal = CreateDefaultInput(distance: 70_000);

        var atResult = HitCalculator.CalculateHitRatio(atOptimal);
        var beyondResult = HitCalculator.CalculateHitRatio(beyondOptimal);

        _output.WriteLine($"At optimal: {atResult.HitRatio:F4} (dist factor {atResult.DistanceFactor:F4})");
        _output.WriteLine($"Beyond optimal: {beyondResult.HitRatio:F4} (dist factor {beyondResult.DistanceFactor:F4})");

        Assert.True(beyondResult.HitRatio < atResult.HitRatio);
        Assert.True(beyondResult.DistanceFactor < 1.0);
    }

    [Fact]
    public void CalculateHitRatio_HighAngularVelocity_ReducesRatio()
    {
        var stationary = CreateDefaultInput(angularVelocityDeg: 0.0);
        var moving = CreateDefaultInput(angularVelocityDeg: 5.0);

        var stationaryResult = HitCalculator.CalculateHitRatio(stationary);
        var movingResult = HitCalculator.CalculateHitRatio(moving);

        _output.WriteLine($"Stationary: {stationaryResult.HitRatio:F4} (tracking {stationaryResult.TrackingFactor:F4})");
        _output.WriteLine($"Moving: {movingResult.HitRatio:F4} (tracking {movingResult.TrackingFactor:F4})");

        Assert.True(movingResult.HitRatio < stationaryResult.HitRatio);
    }

    [Fact]
    public void CalculateHitRatio_NpcMode_AngleAndTrackingAreOne()
    {
        // NPC callers pass 0 for angle and angular velocity
        var input = CreateDefaultInput(angleDeg: 0.0, angularVelocityDeg: 0.0);
        var output = HitCalculator.CalculateHitRatio(input);

        Assert.Equal(1.0, output.AngleFactor);
        Assert.Equal(1.0, output.TrackingFactor);
    }

    [Fact]
    public void CalculateHitRatio_DoesNotClamp_AboveOne()
    {
        // High accuracy + all factors at 1.0 + favorable cross-section can exceed 1.0
        var input = new HitCalculationInput
        {
            BaseAccuracy = 1.5,           // artificially high
            AmmoAccuracyModifier = 1.0,
            AngleDegrees = 0.0,
            OptimalAimingCone = 3.0,
            FalloffAimingCone = 1.5,
            Distance = 0.0,
            OptimalDistance = 50_000,
            FalloffDistance = 20_000,
            AngularVelocityDegrees = 0.0,
            OptimalTracking = 1.0,
            FalloffTracking = 0.5,
            CrossSection = 10000.0,       // large target
            OptimalCrossSectionDiameter = 2.0,
        };

        var output = HitCalculator.CalculateHitRatio(input);

        _output.WriteLine($"HitRatio={output.HitRatio:F4} (should exceed 1.0)");
        Assert.True(output.HitRatio > 1.0, "Hit ratio should not be clamped");
    }

    // ── ComputeAngularVelocity ─────────────────────────────────────

    [Fact]
    public void ComputeAngularVelocity_StationaryTarget_ReturnsZero()
    {
        var target = new Vec3(100, 0, 0);
        var velocity = Vec3.Zero;
        var angVel = Vec3.Zero;

        var result = HitCalculator.ComputeAngularVelocity(target, velocity, angVel);

        Assert.Equal(0.0, result);
    }

    [Fact]
    public void ComputeAngularVelocity_PerpendicularVelocity_ReturnsNonZero()
    {
        var target = new Vec3(100, 0, 0);    // target at 100m on X
        var velocity = new Vec3(0, 10, 0);   // moving along Y at 10m/s
        var angVel = Vec3.Zero;

        var result = HitCalculator.ComputeAngularVelocity(target, velocity, angVel);

        // Expected: |target x vel| / |target|^2 = |(0,0,1000)| / 10000 = 0.1 rad/s
        _output.WriteLine($"Angular velocity: {result:F6} rad/s ({result * 180 / Math.PI:F4} deg/s)");
        Assert.Equal(0.1, result, precision: 6);
    }

    [Fact]
    public void ComputeAngularVelocity_ZeroDistance_ReturnsZero()
    {
        var result = HitCalculator.ComputeAngularVelocity(Vec3.Zero, new Vec3(10, 0, 0), Vec3.Zero);
        Assert.Equal(0.0, result);
    }

    // ── CalculateStasisRange ───────────────────────────────────────

    [Fact]
    public void CalculateStasisRange_HeavyTarget_ReturnsMax()
    {
        var input = new StasisRangeInput
        {
            TargetMass = 200_000,
            HeavyConstructMass = 100_000,
            RangeMin = 50_000,
            RangeMax = 20_000,
            RangeCurvature = 2.0,
        };

        var range = HitCalculator.CalculateStasisRange(input);
        Assert.Equal(20_000, range);
    }

    [Fact]
    public void CalculateStasisRange_LightTarget_ReturnsLongerRange()
    {
        var input = new StasisRangeInput
        {
            TargetMass = 10_000,
            HeavyConstructMass = 100_000,
            RangeMin = 50_000,
            RangeMax = 20_000,
            RangeCurvature = 2.0,
        };

        var range = HitCalculator.CalculateStasisRange(input);

        _output.WriteLine($"Stasis range for light target: {range:F0}m");
        Assert.True(range > 20_000, "Light targets should have longer range than RangeMax");
        Assert.True(range <= 50_000, "Range should not exceed RangeMin");
    }

    [Fact]
    public void CalculateStasisRange_ZeroMass_ReturnsRangeMin()
    {
        var input = new StasisRangeInput
        {
            TargetMass = 0,
            HeavyConstructMass = 100_000,
            RangeMin = 50_000,
            RangeMax = 20_000,
            RangeCurvature = 2.0,
        };

        var range = HitCalculator.CalculateStasisRange(input);

        _output.WriteLine($"Stasis range for zero mass: {range:F0}m");
        Assert.Equal(50_000, range, precision: 1);
    }

    // ── CalculateStasisHit ─────────────────────────────────────────

    [Fact]
    public void CalculateStasisHit_WithinRange_IsHit()
    {
        var output = HitCalculator.CalculateStasisHit(new StasisHitInput
        {
            Distance = 20_000,
            Range = 30_000,
            BaseEffectStrength = 0.8,
        });

        Assert.True(output.IsHit);
        Assert.Equal(0.8, output.EffectStrength, precision: 6);
    }

    [Fact]
    public void CalculateStasisHit_BeyondRange_ButWithinTriple_IsHitWithReducedEffect()
    {
        var output = HitCalculator.CalculateStasisHit(new StasisHitInput
        {
            Distance = 60_000,   // 2x range
            Range = 30_000,
            BaseEffectStrength = 1.0,
        });

        Assert.True(output.IsHit);
        // 0.5^(max(60000-30000, 0)/30000) = 0.5^1 = 0.5
        Assert.Equal(0.5, output.EffectStrength, precision: 6);
    }

    [Fact]
    public void CalculateStasisHit_BeyondTripleRange_IsMiss()
    {
        var output = HitCalculator.CalculateStasisHit(new StasisHitInput
        {
            Distance = 100_000,
            Range = 30_000,
            BaseEffectStrength = 1.0,
        });

        Assert.False(output.IsHit);
    }

    // ── CalculateMissImpact ────────────────────────────────────────

    [Fact]
    public void CalculateMissImpact_DeterministicWithSameInputs()
    {
        var input = new MissImpactInput
        {
            Origin = new Vec3(0, 0, 0),
            Target = new Vec3(1000, 0, 0),
            Size = 16.0,
            MissRange = 0.5,
            RandomAngle = 0.25,
            RandomPerpMagnitude = 0.5,
            RandomAlongMagnitude = 0.5,
        };

        var result1 = HitCalculator.CalculateMissImpact(input);
        var result2 = HitCalculator.CalculateMissImpact(input);

        Assert.Equal(result1.ImpactPosition.X, result2.ImpactPosition.X, precision: 10);
        Assert.Equal(result1.ImpactPosition.Y, result2.ImpactPosition.Y, precision: 10);
        Assert.Equal(result1.ImpactPosition.Z, result2.ImpactPosition.Z, precision: 10);
    }

    [Fact]
    public void CalculateMissImpact_ImpactIsNearTarget()
    {
        var origin = new Vec3(0, 0, 0);
        var target = new Vec3(50_000, 0, 0);

        var output = HitCalculator.CalculateMissImpact(new MissImpactInput
        {
            Origin = origin,
            Target = target,
            Size = 16.0,
            MissRange = 0.5,
            RandomAngle = 0.5,
            RandomPerpMagnitude = 0.5,
            RandomAlongMagnitude = 0.5,
        });

        var distFromTarget = target.Dist(output.ImpactPosition);
        _output.WriteLine($"Miss impact distance from target: {distFromTarget:F2}m");

        // Should be near the target, not at the origin
        Assert.True(output.ImpactPosition.X > 49_000, "Impact should be near target X");
        Assert.True(distFromTarget < 100, "Miss offset should be small relative to distance");
    }

    [Fact]
    public void CalculateMissImpact_DifferentRandoms_ProduceDifferentPositions()
    {
        var baseInput = new MissImpactInput
        {
            Origin = new Vec3(0, 0, 0),
            Target = new Vec3(1000, 0, 0),
            Size = 16.0,
            MissRange = 0.5,
            RandomAngle = 0.1,
            RandomPerpMagnitude = 0.5,
            RandomAlongMagnitude = 0.5,
        };

        var result1 = HitCalculator.CalculateMissImpact(baseInput);

        baseInput.RandomAngle = 0.9;
        var result2 = HitCalculator.CalculateMissImpact(baseInput);

        var dist = result1.ImpactPosition.Dist(result2.ImpactPosition);
        _output.WriteLine($"Distance between miss impacts: {dist:F4}m");
        Assert.True(dist > 0.01, "Different random angles should produce different positions");
    }

    // ── Vec3.Orthogonal ────────────────────────────────────────────

    [Fact]
    public void Vec3_Orthogonal_IsPerpendicularToInput()
    {
        var vectors = new[]
        {
            new Vec3(1, 0, 0),
            new Vec3(0, 1, 0),
            new Vec3(0, 0, 1),
            new Vec3(3, 4, 5),
            new Vec3(-1, 2, -3),
        };

        foreach (var v in vectors)
        {
            var ortho = v.Orthogonal();
            var dot = v.Dot(ortho);
            _output.WriteLine($"v={v} ortho={ortho} dot={dot:E4}");
            Assert.True(Math.Abs(dot) < 1e-10, $"Orthogonal should be perpendicular: dot={dot}");
        }
    }

    [Fact]
    public void Vec3_LengthSquared_MatchesSizeSquared()
    {
        var v = new Vec3(3, 4, 5);
        var size = v.Size();
        Assert.Equal(size * size, v.LengthSquared(), precision: 10);
    }

    // ── Integration: Full NPC Shot Pipeline ────────────────────────

    [Fact]
    public void Integration_NpcShot_AtOptimalRange_HitsWithHighProbability()
    {
        // Simulate what ShootWeaponAction.CalculateHitRatio does for an NPC
        var distance = 50_000.0; // at optimal range
        var crossSection = 5.0;

        var output = HitCalculator.CalculateHitRatio(new HitCalculationInput
        {
            BaseAccuracy = 0.9,
            AmmoAccuracyModifier = 1.0,
            AngleDegrees = 0.0,             // NPC: perfect aim
            OptimalAimingCone = 3.0,
            FalloffAimingCone = 1.5,
            Distance = distance,
            OptimalDistance = 50_000,
            FalloffDistance = 20_000,
            AngularVelocityDegrees = 0.0,   // NPC: no angular velocity tracking
            OptimalTracking = 1.0,
            FalloffTracking = 0.5,
            CrossSection = crossSection,
            OptimalCrossSectionDiameter = 32.0,
        });

        _output.WriteLine(
            $"NPC shot at optimal range: hitRatio={output.HitRatio:F4}\n" +
            $"  accuracy={output.Accuracy:F4} angle={output.AngleFactor:F4} " +
            $"distance={output.DistanceFactor:F4} tracking={output.TrackingFactor:F4} " +
            $"crossSection={output.CrossSectionFactor:F4}");

        // Angle and distance factors should be 1.0 at optimal
        Assert.Equal(1.0, output.AngleFactor);
        Assert.Equal(1.0, output.DistanceFactor);
        Assert.Equal(1.0, output.TrackingFactor);

        // Cross-section factor: sqrt(5 / (16^2 * pi)) * (1-1) + 1 = 1.0 (tracking=1)
        Assert.Equal(1.0, output.CrossSectionFactor, precision: 6);

        // Final ratio should be the accuracy
        Assert.Equal(0.9, output.HitRatio, precision: 6);
    }

    // ── Helper ─────────────────────────────────────────────────────

    private static HitCalculationInput CreateDefaultInput(
        double distance = 50_000,
        double angleDeg = 0.0,
        double angularVelocityDeg = 0.0)
    {
        return new HitCalculationInput
        {
            BaseAccuracy = 0.9,
            AmmoAccuracyModifier = 1.0,
            AngleDegrees = angleDeg,
            OptimalAimingCone = 3.0,
            FalloffAimingCone = 1.5,
            Distance = distance,
            OptimalDistance = 50_000,
            FalloffDistance = 20_000,
            AngularVelocityDegrees = angularVelocityDeg,
            OptimalTracking = 1.0,
            FalloffTracking = 0.5,
            CrossSection = 5.0,
            OptimalCrossSectionDiameter = 32.0,
        };
    }
}
