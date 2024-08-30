using System;
using System.Collections.Generic;
using BotLib.BotClient;
using NQ;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Data;

public class ScriptContext(
    IServiceProvider serviceProvider, 
    ISet<ulong> playerIds, 
    Vec3 sector, 
    Client client
)
{
    public Client Client { get; } = client;
    public IServiceProvider ServiceProvider { get; set; } = serviceProvider;
    public ISet<ulong> PlayerIds { get; set; } = playerIds;
    public Vec3 Sector { get; set; } = sector;
}