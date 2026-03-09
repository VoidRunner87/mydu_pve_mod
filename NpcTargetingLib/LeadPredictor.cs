using NpcCommonLib.Math;

namespace NpcTargetingLib;

/// <summary>
/// Predicts a target's future position using kinematic equations.
/// </summary>
/// <remarks>
/// <para>
/// Uses the standard kinematic equation: <c>p = p0 + v*t + 0.5*a*t^2</c>
/// where t is the prediction time in seconds.
/// </para>
/// <para>
/// Prediction time varies by range tier (matching <c>BehaviorContext.CalculateMovementPredictionSeconds()</c>):
/// <list type="bullet">
///   <item>Outside 2x optimal range: 10 seconds (fast-closing, less prediction needed)</item>
///   <item>Outside 1x optimal range: 30 seconds (moderate prediction)</item>
///   <item>Inside optimal range: 60 seconds (tight engagement, max prediction)</item>
/// </list>
/// </para>
/// </remarks>
public static class LeadPredictor
{
    /// <summary>
    /// Predicts target position at a future time using kinematic equation.
    /// </summary>
    /// <param name="currentPosition">Target's current position in metres.</param>
    /// <param name="velocity">Target's velocity in m/s.</param>
    /// <param name="acceleration">Target's acceleration in m/s².</param>
    /// <param name="predictionSeconds">How far ahead to predict, in seconds.</param>
    /// <returns>Predicted future position in metres.</returns>
    public static Vec3 PredictFuturePosition(
        Vec3 currentPosition, Vec3 velocity, Vec3 acceleration, double predictionSeconds)
    {
        var t = predictionSeconds;
        return new Vec3(
            currentPosition.X + velocity.X * t + 0.5 * acceleration.X * t * t,
            currentPosition.Y + velocity.Y * t + 0.5 * acceleration.Y * t * t,
            currentPosition.Z + velocity.Z * t + 0.5 * acceleration.Z * t * t
        );
    }

    /// <summary>
    /// Calculates prediction seconds based on distance to target relative to weapon optimal range.
    /// </summary>
    /// <param name="distanceToTarget">Distance to target in metres.</param>
    /// <param name="weaponOptimalRange">Weapon's optimal engagement range in metres.</param>
    /// <returns>10, 30, or 60 seconds.</returns>
    public static double CalculatePredictionSeconds(double distanceToTarget, double weaponOptimalRange)
    {
        if (weaponOptimalRange <= 0) return 10;

        if (distanceToTarget > 2 * weaponOptimalRange) return 10;
        if (distanceToTarget > weaponOptimalRange) return 30;
        return 60;
    }
}
