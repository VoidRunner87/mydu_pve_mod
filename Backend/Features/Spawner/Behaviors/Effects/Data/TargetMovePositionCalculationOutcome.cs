using NQ;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Data;

public class TargetMovePositionCalculationOutcome
{
    public bool Valid { get; set; }
    public Vec3 TargetMovePosition { get; set; }

    public static TargetMovePositionCalculationOutcome Invalid() => new();

    public static TargetMovePositionCalculationOutcome MoveToAlternatePosition(Vec3 targetMovePosition) => new()
    {
        Valid = true,
        TargetMovePosition = targetMovePosition,
    };
    
    public static TargetMovePositionCalculationOutcome ValidCalculation(
        Vec3 targetMovePosition
    ) => new()
    {
        Valid = true,
        TargetMovePosition = targetMovePosition,
    };
}