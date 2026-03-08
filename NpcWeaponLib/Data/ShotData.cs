using NpcMovementLib.Data;
using NpcMovementLib.Math;

namespace NpcWeaponLib.Data;

/// <summary>
/// Complete shot data ready for dispatch to the game server.
/// Combines weapon properties (with modifiers applied) and positional context.
/// </summary>
/// <remarks>
/// This replaces both <c>ShootWeaponData</c> and the <c>SentinelWeapon</c> construction
/// from the original <c>AggressiveBehavior.ShootAndCycleAsync</c>.
/// The consumer's <see cref="Interfaces.IShotDispatchService"/> implementation
/// maps this to whatever the game server expects.
/// </remarks>
public class ShotData
{
    // --- Shooter Info ---
    public required string WeaponDisplayName { get; set; }
    public required ConstructId ShooterConstructId { get; set; }
    public required Vec3 ShooterPosition { get; set; }
    public required ulong ShooterConstructSize { get; set; }

    // --- Target Info ---
    public required ConstructId TargetConstructId { get; set; }
    public required Vec3 TargetPosition { get; set; }

    /// <summary>
    /// Local-space hit position on the target construct.
    /// Determined by <see cref="Interfaces.IHitPositionService"/> or random fallback.
    /// </summary>
    public required Vec3 HitPosition { get; set; }

    // --- Weapon Properties (modifiers already applied) ---
    public required double Damage { get; set; }
    public required double Range { get; set; }
    public required double BaseAccuracy { get; set; }
    public required double BaseOptimalDistance { get; set; }
    public required double BaseOptimalTracking { get; set; }
    public required double BaseOptimalAimingCone { get; set; }
    public required double FalloffDistance { get; set; }
    public required double FalloffTracking { get; set; }
    public required double FalloffAimingCone { get; set; }
    public required double OptimalCrossSectionDiameter { get; set; }
    public required double FireCooldown { get; set; }
    public required double CrossSection { get; set; }

    // --- Ammo ---
    public required string AmmoItemTypeName { get; set; }
    public required string WeaponItemTypeName { get; set; }

    /// <summary>Number of functional weapons firing simultaneously.</summary>
    public required int WeaponCount { get; set; }
}
