using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors;

public static class ConstructBehaviorExtensions
{
    public static IConstructBehavior WithErrorHandler(this IConstructBehavior constructBehavior)
    {
        return new ErrorHandlerBehavior(constructBehavior);
    }
}