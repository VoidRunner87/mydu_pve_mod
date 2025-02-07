using System.Linq;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Interfaces;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Services;

public class HighestThreatRadarTargetEffect : ISelectRadarTargetEffect
{
    public ScanContact? GetTarget(ISelectRadarTargetEffect.Params @params)
    {
        var constructId = @params.Context.GetHighestThreatConstruct();

        return @params.Contacts.FirstOrDefault(x => x.ConstructId == constructId);
    }
}