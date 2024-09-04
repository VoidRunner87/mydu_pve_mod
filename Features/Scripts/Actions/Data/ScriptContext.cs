using System;
using System.Collections.Generic;
using NQ;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Data;

public class ScriptContext(
    IServiceProvider serviceProvider, 
    ISet<ulong> playerIds, 
    Vec3 sector 
)
{
    public IServiceProvider ServiceProvider { get; set; } = serviceProvider;
    public ISet<ulong> PlayerIds { get; set; } = playerIds;
    public Vec3 Sector { get; set; } = sector;
    public ulong? ConstructId { get; set; }
}