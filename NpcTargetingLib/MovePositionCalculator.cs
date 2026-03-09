using NpcCommonLib.Math;

namespace NpcTargetingLib;

/// <summary>
/// Calculates the move-to position for the NPC, combining target position
/// with random offset and optional lead prediction.
/// </summary>
/// <remarks>
/// Ported from <c>CalculateTargetMovePositionWithOffsetEffect</c>.
/// The original queries the game server for target position; this version
/// takes the target position as input. The random offset is regenerated
/// every 30 seconds to prevent the NPC from flying in a straight line.
/// </remarks>
public class MovePositionCalculator
{
    private readonly Random _random;
    private Vec3 _offset;
    private DateTime? _lastOffsetUpdate;

    /// <summary>How often to regenerate the random offset. Default: 30 seconds.</summary>
    public TimeSpan OffsetRefreshInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Creates a new move position calculator with an optional random number generator.
    /// </summary>
    /// <param name="random">RNG instance for offset generation; if null, a new <see cref="Random"/> is created.</param>
    public MovePositionCalculator(Random? random = null)
    {
        _random = random ?? new Random();
    }

    /// <summary>
    /// Calculates the move-to position given the target's position and movement data.
    /// </summary>
    /// <param name="targetPosition">Target's current position in metres.</param>
    /// <param name="targetVelocity">Target's velocity in m/s.</param>
    /// <param name="targetAcceleration">Target's acceleration in m/s².</param>
    /// <param name="predictionSeconds">Lead prediction lookahead time.</param>
    /// <param name="approachDistance">Desired engagement distance in metres (offset magnitude).</param>
    /// <param name="usePrediction">Whether to apply lead prediction. Default: false (matching current backend which has it commented out).</param>
    /// <returns>The position the NPC should navigate toward.</returns>
    public Vec3 Calculate(
        Vec3 targetPosition,
        Vec3 targetVelocity,
        Vec3 targetAcceleration,
        double predictionSeconds,
        double approachDistance,
        bool usePrediction = false)
    {
        // Refresh random offset periodically
        var now = DateTime.UtcNow;
        if (_lastOffsetUpdate == null || (now - _lastOffsetUpdate.Value) > OffsetRefreshInterval)
        {
            _offset = RandomDirectionVec3() * (approachDistance / 2);
            _lastOffsetUpdate = now;
        }

        var basePosition = targetPosition;

        if (usePrediction)
        {
            basePosition = LeadPredictor.PredictFuturePosition(
                targetPosition, targetVelocity, targetAcceleration, predictionSeconds);
        }

        return basePosition + _offset;
    }

    /// <summary>Resets the offset timer, forcing a new offset on next call.</summary>
    public void ResetOffset() => _lastOffsetUpdate = null;

    private Vec3 RandomDirectionVec3()
    {
        // Generate a random unit vector (uniform on sphere)
        var theta = _random.NextDouble() * 2 * Math.PI;
        var phi = Math.Acos(2 * _random.NextDouble() - 1);
        return new Vec3(
            Math.Sin(phi) * Math.Cos(theta),
            Math.Sin(phi) * Math.Sin(theta),
            Math.Cos(phi)
        );
    }
}
