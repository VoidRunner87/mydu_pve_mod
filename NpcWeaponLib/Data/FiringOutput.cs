namespace NpcWeaponLib.Data;

/// <summary>
/// Result of a single firing tick from <see cref="FiringSimulator.Tick"/>.
/// </summary>
public class FiringOutput
{
    /// <summary>Whether the NPC should fire this tick.</summary>
    public required bool ShouldFire { get; set; }

    /// <summary>
    /// The shot to dispatch if <see cref="ShouldFire"/> is true. Null otherwise.
    /// Contains all data needed to send the shot to the game server.
    /// </summary>
    public ShotData? Shot { get; set; }

    /// <summary>The weapon that was selected for this tick (even if not firing yet).</summary>
    public WeaponStats? SelectedWeapon { get; set; }

    /// <summary>Current fire interval in seconds (for diagnostics/UI).</summary>
    public double FireInterval { get; set; }

    /// <summary>Accumulated time since last shot in seconds (for diagnostics/UI).</summary>
    public double AccumulatedTime { get; set; }

    /// <summary>Ratio of functional weapons to total (0-1).</summary>
    public double FunctionalWeaponFactor { get; set; }

    /// <summary>Reason firing was suppressed, if <see cref="ShouldFire"/> is false.</summary>
    public FiringSuppressedReason? SuppressedReason { get; set; }
}

/// <summary>Why the NPC did not fire this tick.</summary>
public enum FiringSuppressedReason
{
    /// <summary>NPC is dead.</summary>
    NotAlive,
    /// <summary>No target assigned.</summary>
    NoTarget,
    /// <summary>No weapons on construct.</summary>
    NoWeapons,
    /// <summary>All weapons destroyed.</summary>
    AllWeaponsDestroyed,
    /// <summary>No compatible ammo found for configured tier/variant.</summary>
    NoCompatibleAmmo,
    /// <summary>Target beyond max engagement range.</summary>
    OutOfRange,
    /// <summary>Fire interval not yet reached — accumulating time.</summary>
    CooldownNotReached,
}
