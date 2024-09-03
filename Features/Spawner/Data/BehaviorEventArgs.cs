using System;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

namespace Mod.DynamicEncounters.Features.Spawner.Data;

public class BehaviorEventArgs(ulong constructId, IConstructDefinition constructDefinition, BehaviorContext context) : EventArgs
{
    public ulong ConstructId { get; set; } = constructId;
    public IConstructDefinition ConstructDefinition { get; } = constructDefinition;
    public BehaviorContext Context { get; } = context;
}