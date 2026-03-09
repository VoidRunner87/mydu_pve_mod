using NpcCommonLib.Data;

namespace NpcTargetingLib.Strategies;

/// <summary>
/// Selects a random contact and holds that selection for a configurable duration.
/// </summary>
/// <remarks>
/// Ported from <c>RandomSelectRadarTargetEffect</c>. Stateful — tracks
/// accumulated time and last selection. Re-rolls after <c>DecisionHoldSeconds</c>.
/// </remarks>
public class RandomTargetStrategy : ITargetSelectionStrategy
{
    private readonly Random _random;
    private double _accumulatedTime;
    private ConstructId? _lastSelectedId;

    /// <summary>
    /// Creates a new random target strategy with an optional random number generator.
    /// </summary>
    /// <param name="random">RNG instance; if null, a new <see cref="Random"/> is created.</param>
    public RandomTargetStrategy(Random? random = null)
    {
        _random = random ?? new Random();
    }

    /// <inheritdoc/>
    public ScanContact? SelectTarget(TargetSelectionParams @params)
    {
        _accumulatedTime += @params.DeltaTime;

        // Check if current selection is still valid (on radar)
        ScanContact? current = null;
        if (_lastSelectedId != null)
            current = @params.Contacts.FirstOrDefault(c => c.ConstructId == _lastSelectedId.Value);

        // Re-roll if: no selection, selection lost, or hold time expired
        if (current == null || _accumulatedTime > @params.DecisionHoldSeconds)
        {
            if (@params.Contacts.Count == 0)
            {
                _lastSelectedId = null;
                _accumulatedTime = 0;
                return null;
            }

            var index = _random.Next(@params.Contacts.Count);
            current = @params.Contacts[index];
            _lastSelectedId = current.ConstructId;
            _accumulatedTime = 0;
        }

        return current;
    }
}
