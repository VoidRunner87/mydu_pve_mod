using NpcCommonLib.Data;
using NpcTargetingLib.Data;

namespace NpcTargetingLib;

/// <summary>
/// Calculates which attacker poses the highest threat based on recent damage history.
/// Pure static functions with no side effects.
/// </summary>
/// <remarks>
/// Ported from <c>BehaviorContext.GetHighestThreatConstruct()</c>.
/// Uses a 1-minute window (not the 10-minute retention window) for threat ranking,
/// falling back to the closest radar contact if no recent damage exists.
/// </remarks>
public static class ThreatCalculator
{
    /// <summary>
    /// Threat assessment window. The original uses 1 minute for threat ranking
    /// (shorter than the 10-minute damage retention window).
    /// </summary>
    public static readonly TimeSpan DefaultThreatWindow = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Returns the construct ID that dealt the most total damage within the threat window.
    /// Falls back to the closest contact if no damage was received recently.
    /// Returns null if no contacts and no damage history.
    /// </summary>
    /// <param name="damageHistory">Recent damage events (from DamageTracker).</param>
    /// <param name="contacts">Current radar contacts.</param>
    /// <param name="threatWindow">How far back to consider damage. Default: 1 minute.</param>
    public static ConstructId? GetHighestThreat(
        IReadOnlyList<DamageEvent> damageHistory,
        IReadOnlyList<ScanContact> contacts,
        TimeSpan? threatWindow = null)
    {
        var window = threatWindow ?? DefaultThreatWindow;
        var cutoff = DateTime.UtcNow - window;

        var highestThreat = damageHistory
            .Where(e => e.Timestamp > cutoff)
            .GroupBy(e => e.AttackerConstructId)
            .Select(g => new { ConstructId = g.Key, TotalDamage = g.Sum(e => e.Damage) })
            .OrderByDescending(x => x.TotalDamage)
            .FirstOrDefault();

        if (highestThreat != null)
            return highestThreat.ConstructId;

        // Fallback: closest contact
        return GetClosestContact(contacts);
    }

    /// <summary>
    /// Returns the construct ID of the nearest radar contact, or null if none.
    /// </summary>
    public static ConstructId? GetClosestContact(IReadOnlyList<ScanContact> contacts)
    {
        if (contacts.Count == 0) return null;
        return contacts.MinBy(c => c.Distance)?.ConstructId;
    }
}
