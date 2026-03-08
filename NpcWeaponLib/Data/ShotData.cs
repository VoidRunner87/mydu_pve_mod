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

    /// <summary>Display name of the weapon that fired (e.g., "Rare Military Small Railgun s").</summary>
    public required string WeaponDisplayName { get; set; }

    /// <summary>Construct identifier of the NPC that fired the shot.</summary>
    public required ConstructId ShooterConstructId { get; set; }

    /// <summary>Shooter's world-space position in metres at time of firing.</summary>
    public required Vec3 ShooterPosition { get; set; }

    /// <summary>Shooter's construct bounding size. Used as fallback for hit position calculation.</summary>
    public required ulong ShooterConstructSize { get; set; }

    // --- Target Info ---

    /// <summary>Construct identifier of the target being fired upon.</summary>
    public required ConstructId TargetConstructId { get; set; }

    /// <summary>Target's world-space position in metres at time of firing.</summary>
    public required Vec3 TargetPosition { get; set; }

    /// <summary>
    /// Local-space hit position on the target construct.
    /// Determined by <see cref="Interfaces.IHitPositionService"/> or random fallback.
    /// </summary>
    public required Vec3 HitPosition { get; set; }

    // --- Weapon Properties (modifiers already applied) ---

    /// <summary>Damage per shot after <see cref="WeaponModifiers.Damage"/> is applied.</summary>
    public required double Damage { get; set; }

    /// <summary>Effective engagement range in metres: optimal distance + falloff distance (both modified).</summary>
    public required double Range { get; set; }

    /// <summary>Hit probability at optimal range (0-1), after <see cref="WeaponModifiers.Accuracy"/> is applied.</summary>
    public required double BaseAccuracy { get; set; }

    /// <summary>Optimal engagement distance in metres, after <see cref="WeaponModifiers.OptimalDistance"/> is applied.</summary>
    public required double BaseOptimalDistance { get; set; }

    /// <summary>Tracking effectiveness at optimal range, after <see cref="WeaponModifiers.OptimalTracking"/> is applied.</summary>
    public required double BaseOptimalTracking { get; set; }

    /// <summary>Aiming cone half-angle at optimal range, after <see cref="WeaponModifiers.OptimalAimingCone"/> is applied.</summary>
    public required double BaseOptimalAimingCone { get; set; }

    /// <summary>Distance beyond optimal where effectiveness degrades, in metres, after modifier is applied.</summary>
    public required double FalloffDistance { get; set; }

    /// <summary>Tracking degradation beyond optimal range, after <see cref="WeaponModifiers.FalloffTracking"/> is applied.</summary>
    public required double FalloffTracking { get; set; }

    /// <summary>Aiming cone expansion beyond optimal range, after <see cref="WeaponModifiers.FalloffAimingCone"/> is applied.</summary>
    public required double FalloffAimingCone { get; set; }

    /// <summary>Ideal target cross-section diameter in metres (not modified).</summary>
    public required double OptimalCrossSectionDiameter { get; set; }

    /// <summary>Effective fire cooldown in seconds, as computed by <see cref="WeaponFireRateCalculator"/>.</summary>
    public required double FireCooldown { get; set; }

    /// <summary>Target cross-section value used for hit calculation. Defaults to 5.</summary>
    public required double CrossSection { get; set; }

    // --- Ammo ---

    /// <summary>Internal type name of the ammo selected for this shot.</summary>
    public required string AmmoItemTypeName { get; set; }

    /// <summary>Internal type name of the weapon that fired this shot.</summary>
    public required string WeaponItemTypeName { get; set; }

    /// <summary>Number of functional weapons firing simultaneously.</summary>
    public required int WeaponCount { get; set; }
}
