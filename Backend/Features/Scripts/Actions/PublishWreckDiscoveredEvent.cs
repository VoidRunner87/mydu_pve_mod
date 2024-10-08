using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Events.Data;
using Mod.DynamicEncounters.Features.Events.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Services;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

[ScriptActionName(ActionName, Description = Description)]
public class PublishWreckDiscoveredEvent : IScriptAction
{
    public const string ActionName = "publish-wreck-discovered-event";
    public const string Description = "Tracks that a wreck was discovered. Useful for event handlers";
    public string Name => ActionName;

    public string GetKey() => Name;
    
    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var provider = context.ServiceProvider;
        var eventService = provider.GetRequiredService<IEventService>();

        foreach (var playerId in context.PlayerIds)
        {
            await eventService.PublishAsync(
                new WreckDiscoveredEvent(
                    playerId,
                    context.Sector,
                    context.ConstructId,
                    context.PlayerIds.Count
                )
            );    
        }
        
        return ScriptActionResult.Successful();
    }
}