using NpcCommonLib.Data;
using NpcTargetingLib.Data;

namespace NpcTargetingLib.Strategies;

/// <summary>
/// Strategy interface for selecting a target from radar contacts.
/// </summary>
/// <remarks>
/// Ported from <c>ISelectRadarTargetEffect</c>. The original uses the "effect" system
/// with a <c>BehaviorContext</c> parameter; this version uses pure inputs.
/// </remarks>
public interface ITargetSelectionStrategy
{
    /// <summary>
    /// Selects a target from the available contacts.
    /// Returns null if no suitable target found.
    /// </summary>
    /// <param name="params">Selection parameters including contacts and damage history.</param>
    ScanContact? SelectTarget(TargetSelectionParams @params);
}

/// <summary>
/// Input parameters for target selection strategies.
/// </summary>
public class TargetSelectionParams
{
    /// <summary>Current radar contacts, sorted by ascending distance.</summary>
    public required IReadOnlyList<ScanContact> Contacts { get; set; }

    /// <summary>Recent damage history for threat-based selection.</summary>
    public required IReadOnlyList<DamageEvent> DamageHistory { get; set; }

    /// <summary>Seconds since last tick (for time-based strategies like Random hold).</summary>
    public required double DeltaTime { get; set; }

    /// <summary>How long to hold a random selection before re-rolling. Default: 30s.</summary>
    public double DecisionHoldSeconds { get; set; } = 30;
}
