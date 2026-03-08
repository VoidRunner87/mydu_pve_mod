using NpcCommonLib.Data;

namespace NpcTargetingLib.Data;

/// <summary>
/// Records a single damage event dealt to the NPC by an attacker.
/// </summary>
/// <remarks>
/// Ported from <c>Mod.DynamicEncounters.Features.Spawner.Data.DamageDealtData</c>.
/// Used by <see cref="DamageTracker"/> to maintain a rolling damage history
/// for threat assessment. The original is registered via an HTTP endpoint
/// (<c>BehaviorContextController.RegisterDamage</c>) when the game detects
/// a shot impact on this NPC.
/// </remarks>
public class DamageEvent
{
    /// <summary>Construct ID of the attacker that dealt this damage.</summary>
    public required ConstructId AttackerConstructId { get; set; }

    /// <summary>Player ID of the attacker (0 if NPC-on-NPC damage).</summary>
    public required ulong AttackerPlayerId { get; set; }

    /// <summary>Damage amount dealt in this event.</summary>
    public required double Damage { get; set; }

    /// <summary>
    /// Damage type descriptor (e.g., "shield-hit", "kinetic", "thermic").
    /// Used for logging and analytics, not for targeting decisions.
    /// </summary>
    public required string Type { get; set; }

    /// <summary>UTC timestamp when this damage was dealt.</summary>
    public required DateTime Timestamp { get; set; }
}
