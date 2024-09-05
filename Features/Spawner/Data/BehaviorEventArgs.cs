using System;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

namespace Mod.DynamicEncounters.Features.Spawner.Data;

public class BehaviorEventArgs(ulong constructId, IPrefab prefab, BehaviorContext context) : EventArgs
{
    public ulong ConstructId { get; set; } = constructId;
    public IPrefab Prefab { get; } = prefab;
    public BehaviorContext Context { get; } = context;
}