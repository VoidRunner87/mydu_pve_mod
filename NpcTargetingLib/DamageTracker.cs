using NpcTargetingLib.Data;

namespace NpcTargetingLib;

/// <summary>
/// Thread-safe rolling damage history for an NPC construct.
/// Tracks who dealt damage and when, for threat assessment.
/// </summary>
/// <remarks>
/// Ported from <c>BehaviorContext.DamageHistory</c> (a <c>ConcurrentBag</c>)
/// and <c>RegisterDamage()</c> / <c>GetRecentDamageHistory()</c>.
/// The original prunes to a 10-minute window on each registration;
/// this version does the same.
/// </remarks>
public class DamageTracker
{
    private readonly object _lock = new();
    private List<DamageEvent> _events = [];

    /// <summary>
    /// How far back to retain damage events. Default: 10 minutes
    /// (matching the original <c>BehaviorContext</c>).
    /// </summary>
    public TimeSpan RetentionWindow { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Registers a damage event and prunes expired entries.
    /// Thread-safe.
    /// </summary>
    public void RegisterDamage(DamageEvent damage)
    {
        lock (_lock)
        {
            var cutoff = DateTime.UtcNow - RetentionWindow;
            _events = _events.Where(e => e.Timestamp > cutoff).ToList();
            _events.Add(damage);
        }
    }

    /// <summary>
    /// Returns all damage events within the retention window.
    /// </summary>
    public IReadOnlyList<DamageEvent> GetRecentHistory()
    {
        lock (_lock)
        {
            var cutoff = DateTime.UtcNow - RetentionWindow;
            return _events.Where(e => e.Timestamp > cutoff).ToList();
        }
    }

    /// <summary>
    /// Returns all damage events within a custom time window.
    /// </summary>
    /// <param name="window">How far back to look.</param>
    public IReadOnlyList<DamageEvent> GetHistory(TimeSpan window)
    {
        lock (_lock)
        {
            var cutoff = DateTime.UtcNow - window;
            return _events.Where(e => e.Timestamp > cutoff).ToList();
        }
    }

    /// <summary>Clears all damage history.</summary>
    public void Clear()
    {
        lock (_lock) { _events.Clear(); }
    }
}
