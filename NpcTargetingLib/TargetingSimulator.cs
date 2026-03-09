using NpcCommonLib.Data;
using NpcTargetingLib.Data;
using NpcTargetingLib.Strategies;

namespace NpcTargetingLib;

/// <summary>
/// Main entry point for NPC targeting simulation. Orchestrates target selection,
/// threat assessment, and move position calculation into a single <see cref="Tick"/> call.
/// </summary>
/// <remarks>
/// <para>
/// Ported from <c>SelectTargetBehavior.TickAsync()</c>. The original mixes DI,
/// radar scanning, safe zone filtering, and game-server calls. This version
/// is pure: it takes pre-scanned contacts and damage history as input.
/// </para>
/// <para>
/// Each call to <see cref="Tick"/> performs:
/// <list type="number">
///   <item>Run the <see cref="ITargetSelectionStrategy"/> to pick a target from contacts.</item>
///   <item>Calculate prediction seconds based on distance vs weapon optimal range.</item>
///   <item>Compute move-to position via <see cref="MovePositionCalculator"/>.</item>
///   <item>Return <see cref="TargetingOutput"/> with selected target, move position, and diagnostics.</item>
/// </list>
/// </para>
/// </remarks>
public class TargetingSimulator
{
    private readonly ITargetSelectionStrategy _strategy;
    private readonly MovePositionCalculator _moveCalculator;
    private readonly DamageTracker _damageTracker;

    /// <summary>
    /// Creates a targeting simulator with the specified strategy.
    /// Defaults to <see cref="HighestThreatTargetStrategy"/> (matching the game's default).
    /// </summary>
    public TargetingSimulator(
        ITargetSelectionStrategy? strategy = null,
        DamageTracker? damageTracker = null,
        MovePositionCalculator? moveCalculator = null)
    {
        _strategy = strategy ?? new HighestThreatTargetStrategy();
        _damageTracker = damageTracker ?? new DamageTracker();
        _moveCalculator = moveCalculator ?? new MovePositionCalculator();
    }

    /// <summary>The damage tracker used for threat assessment. Expose so consumers can register damage.</summary>
    public DamageTracker DamageTracker => _damageTracker;

    /// <summary>
    /// Processes a single targeting tick.
    /// </summary>
    /// <param name="input">All inputs for this targeting tick (position, contacts, weapon data).</param>
    /// <returns>A <see cref="TargetingOutput"/> containing the selected target and move-to position.</returns>
    public TargetingOutput Tick(TargetingInput input)
    {
        // --- No contacts ---
        if (input.Contacts.Count == 0)
        {
            return new TargetingOutput
            {
                HasTarget = false,
                Reason = NoTargetReason.NoContacts,
                MoveToPosition = input.StartPosition,
            };
        }

        // --- Run strategy ---
        var selectionParams = new TargetSelectionParams
        {
            Contacts = input.Contacts,
            DamageHistory = _damageTracker.GetRecentHistory(),
            DeltaTime = input.DeltaTime,
            DecisionHoldSeconds = input.DecisionHoldSeconds,
        };

        var selected = _strategy.SelectTarget(selectionParams);

        if (selected == null)
        {
            return new TargetingOutput
            {
                HasTarget = false,
                Reason = NoTargetReason.NoContacts,
                MoveToPosition = input.StartPosition,
            };
        }

        // --- Visibility check ---
        if (selected.Distance > input.MaxVisibilityDistance)
        {
            return new TargetingOutput
            {
                HasTarget = false,
                Reason = NoTargetReason.AllOutOfRange,
                MoveToPosition = input.StartPosition,
            };
        }

        // --- Prediction ---
        var predictionSeconds = LeadPredictor.CalculatePredictionSeconds(
            selected.Distance, input.WeaponOptimalRange);

        var moveToPosition = _moveCalculator.Calculate(
            targetPosition: selected.Position,
            targetVelocity: input.TargetLinearVelocity,
            targetAcceleration: input.TargetAcceleration,
            predictionSeconds: predictionSeconds,
            approachDistance: input.WeaponOptimalRange
        );

        return new TargetingOutput
        {
            HasTarget = true,
            TargetConstructId = selected.ConstructId,
            TargetPosition = selected.Position,
            MoveToPosition = moveToPosition,
            TargetDistance = selected.Distance,
            PredictionSeconds = predictionSeconds,
        };
    }
}
