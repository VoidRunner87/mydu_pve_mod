using System;
using Mod.DynamicEncounters.Features.Spawner.Data;
using NQ;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Data;

public class ConstructHandleItem
{
    public Guid Id { get; set; }
    public ulong ConstructId { get; set; }
    public Vec3 Sector { get; set; }
    public Guid ConstructDefinitionId { get; set; }
    
    public ConstructDefinitionItem? ConstructDefinitionItem { get; set; }
}