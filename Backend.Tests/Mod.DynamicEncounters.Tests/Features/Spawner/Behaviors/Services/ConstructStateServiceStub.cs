using Mod.DynamicEncounters.Features.Spawner.Behaviors.Data;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using NSubstitute;

namespace Mod.DynamicEncounters.Tests.Features.Spawner.Behaviors.Services;

public static class ConstructStateServiceStubExtensions
{
    public static void WithState(this IConstructStateService service, string type, ulong constructId, ConstructStateOutcome outcome)
    {
        service.Find(type, constructId)
            .Returns(outcome);
    }
}