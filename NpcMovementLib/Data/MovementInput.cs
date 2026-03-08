using System.Numerics;
using NpcMovementLib.Math;

namespace NpcMovementLib.Data;

public class MovementInput
{
    // Current NPC state
    public Vec3 Position { get; set; }
    public Vec3 Velocity { get; set; }
    public Quaternion Rotation { get; set; } = Quaternion.Identity;

    // Target
    public Vec3 TargetMovePosition { get; set; }

    // Timing
    public double DeltaTime { get; set; }

    // Movement parameters
    public double AccelerationG { get; set; } = 15;
    public double MaxSpeedKph { get; set; } = 20000;
    public double MinSpeedKph { get; set; } = 2000;
    public float RotationSpeed { get; set; } = 0.5f;
    public double RealismFactor { get; set; }

    // Engine
    public double EnginePower { get; set; } = 1;
    public bool IsBraking { get; set; }

    // Velocity goal inputs
    public VelocityModifiers Modifiers { get; set; } = new();
    public double TargetDistance { get; set; }
    public Vec3 TargetLinearVelocity { get; set; }
    public double WeaponOptimalRange { get; set; }
    public bool HasOverrideTargetMovePosition { get; set; }
    public double OverrideMovePositionDistance { get; set; }

    // Booster
    public bool BoosterActive { get; set; }

    // For BurnToTarget delta-V clamping
    public Vec3? PreviousVelocity { get; set; }

    // Derived helpers
    public double MinVelocity => MinSpeedKph / 3.6d;
    public double MaxVelocity => MaxSpeedKph / 3.6d;
    public double AccelerationMps => GetAccelerationMps();
    public double TargetMoveDistance => Position.Dist(TargetMovePosition);

    public double GetAccelerationG()
    {
        var boosterG = 0d;
        if (Modifiers.BoosterEnabled && BoosterActive)
        {
            boosterG = Modifiers.BoosterAccelerationG;
        }

        return AccelerationG + boosterG;
    }

    public double GetAccelerationMps() => GetAccelerationG() * 3.6d;
}
