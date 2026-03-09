using NpcCommonLib.Data;
using NpcCommonLib.Math;

namespace NpcTargetingLib.Data;

/// <summary>
/// Result of a single targeting tick from TargetingSimulator.Tick.
/// </summary>
public class TargetingOutput
{
    /// <summary>Whether a valid target was selected.</summary>
    public required bool HasTarget { get; set; }

    /// <summary>The selected target's construct ID. Null if no target.</summary>
    public ConstructId? TargetConstructId { get; set; }

    /// <summary>
    /// The position the NPC should move toward. This may include lead prediction
    /// offset and random jitter — it is NOT simply the target's raw position.
    /// </summary>
    public Vec3 MoveToPosition { get; set; }

    /// <summary>
    /// The target's raw position (without prediction/offset).
    /// Used for distance calculations and weapon ranging.
    /// </summary>
    public Vec3 TargetPosition { get; set; }

    /// <summary>Distance from NPC to target in metres.</summary>
    public double TargetDistance { get; set; }

    /// <summary>
    /// Prediction seconds used for lead calculation this tick.
    /// 10s (far), 30s (medium), 60s (close to optimal range).
    /// </summary>
    public double PredictionSeconds { get; set; }

    /// <summary>Reason no target was selected, if HasTarget is false.</summary>
    public NoTargetReason? Reason { get; set; }
}

/// <summary>Why no target was selected this tick.</summary>
public enum NoTargetReason
{
    /// <summary>No radar contacts within scan range.</summary>
    NoContacts,
    /// <summary>All contacts are beyond max visibility distance.</summary>
    AllOutOfRange,
    /// <summary>Target construct no longer exists in contacts list.</summary>
    TargetLost,
}
