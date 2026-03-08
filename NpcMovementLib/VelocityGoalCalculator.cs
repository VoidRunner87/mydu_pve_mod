using NpcMovementLib.Data;
using NpcMovementLib.Math;

namespace NpcMovementLib;

public static class VelocityGoalCalculator
{
    public class VelocityGoalInput
    {
        public required double Distance { get; init; }
        public required double TargetDistance { get; init; }
        public required Vec3 TargetLinearVelocity { get; init; }
        public required Vec3 NpcVelocity { get; init; }
        public required double MinVelocity { get; init; }
        public required double MaxVelocity { get; init; }
        public required double WeaponOptimalRange { get; init; }
        public required VelocityModifiers Modifiers { get; init; }
        public required bool HasOverrideTargetMovePosition { get; init; }
        public required double OverrideMovePositionDistance { get; init; }
        public required double BrakingDistance { get; init; }
    }

    public static double Calculate(VelocityGoalInput input)
    {
        if (!input.Modifiers.Enabled) return input.MaxVelocity;
        if (input.HasOverrideTargetMovePosition) return CalculateOverrideMoveVelocityGoal(input);

        var npcDirection = input.NpcVelocity.NormalizeSafe();
        var targetDirection = input.TargetLinearVelocity.NormalizeSafe();
        var velocityWithTargetDotProduct = npcDirection.Dot(targetDirection);
        var oppositeVector = velocityWithTargetDotProduct < 0;

        if (input.Distance > input.Modifiers.GetFarDistanceM())
        {
            return input.MaxVelocity;
        }

        var isOutsideDoubleOptimalRange = input.TargetDistance > input.WeaponOptimalRange * 2;
        var isOutsideOptimalRange = input.TargetDistance > input.WeaponOptimalRange;

        if (isOutsideDoubleOptimalRange || input.Distance > input.BrakingDistance * input.Modifiers.BrakeDistanceFactor)
        {
            var baseVelocity = GetOutsideOfOptimalRange2XTargetVelocity(input);
            return oppositeVector
                ? baseVelocity * input.Modifiers.OutsideOptimalRange2X.Negative
                : baseVelocity * input.Modifiers.OutsideOptimalRange2X.Positive;
        }

        if (isOutsideOptimalRange)
        {
            var baseVelocity = GetOutsideOfOptimalRangeTargetVelocity(input);
            return oppositeVector
                ? baseVelocity * input.Modifiers.OutsideOptimalRange.Negative
                : baseVelocity * input.Modifiers.OutsideOptimalRange.Positive;
        }

        if (input.Distance < input.Modifiers.TooCloseDistanceM)
        {
            return input.MaxVelocity;
        }

        var insideVelocity = GetInsideOfOptimalRangeTargetVelocity(input);
        return oppositeVector
            ? insideVelocity * input.Modifiers.InsideOptimalRange.Negative
            : insideVelocity * input.Modifiers.InsideOptimalRange.Positive;
    }

    private static double CalculateOverrideMoveVelocityGoal(VelocityGoalInput input)
    {
        if (input.OverrideMovePositionDistance <= input.BrakingDistance * input.Modifiers.BrakeDistanceFactor)
        {
            return 0d;
        }

        return input.MaxVelocity;
    }

    private static double GetOutsideOfOptimalRange2XTargetVelocity(VelocityGoalInput input)
    {
        if (input.TargetLinearVelocity.Size() < input.MinVelocity)
        {
            return input.MaxVelocity / input.Modifiers.OutsideOptimalRange2XAlpha;
        }

        return System.Math.Clamp(input.TargetLinearVelocity.Size(), input.MinVelocity, input.MaxVelocity);
    }

    private static double GetOutsideOfOptimalRangeTargetVelocity(VelocityGoalInput input)
    {
        if (input.TargetLinearVelocity.Size() < input.MinVelocity)
        {
            return input.MaxVelocity / input.Modifiers.OutsideOptimalRangeAlpha;
        }

        return System.Math.Clamp(input.TargetLinearVelocity.Size(), input.MinVelocity, input.MaxVelocity);
    }

    private static double GetInsideOfOptimalRangeTargetVelocity(VelocityGoalInput input)
    {
        if (input.TargetLinearVelocity.Size() < input.MinVelocity)
        {
            return input.MinVelocity;
        }

        return System.Math.Clamp(input.TargetLinearVelocity.Size(), input.MinVelocity, input.MaxVelocity);
    }
}
