using System.Linq;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Interfaces;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Services;

public class ClosestSelectRadarTargetEffect : ISelectRadarTargetEffect
{
    public ScanContact? GetTarget(ISelectRadarTargetEffect.Params @params)
    {
        return @params.Contacts.MinBy(x => x.Distance);
    }
}