using System;
using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Data;

public class ShortLivedBehaviorContextEntry(BehaviorContext behaviorContext, DateTime expiresAt)
{
    public BehaviorContext BehaviorContext { get; } = behaviorContext;
    public DateTime ExpiresAt { get; set; } = expiresAt;
}