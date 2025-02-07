using System;
using System.Linq;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Services;

public class RandomSelectRadarTargetEffect : ISelectRadarTargetEffect
{
    private Random Random { get; } = new();
    private double AccumulatedDeltaTime { get; set; }
    private ScanContact? LastSelectedTarget { get; set; }
    
    public ScanContact? GetTarget(ISelectRadarTargetEffect.Params @params)
    {
        AccumulatedDeltaTime += @params.Context.DeltaTime;

        if (LastSelectedTarget == null || AccumulatedDeltaTime > @params.DecisionTimeSeconds)
        {
            if (!@params.Contacts.Any())
            {
                LastSelectedTarget = null;
                AccumulatedDeltaTime = 0;
                return LastSelectedTarget;
            }
            
            LastSelectedTarget = Random.PickOneAtRandom(@params.Contacts);
            AccumulatedDeltaTime = 0;
        }
        
        return LastSelectedTarget;
    }
}