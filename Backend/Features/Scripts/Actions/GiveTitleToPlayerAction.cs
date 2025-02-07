using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.NQ.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Services;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

[ScriptActionName(ActionName)]
public class GiveTitleToPlayerAction(ScriptActionItem actionItem) : IScriptAction
{
    public const string ActionName = "give-title";
    
    public string Name => ActionName;

    public string GetKey() => Name;

    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var provider = context.ServiceProvider;
        var playerService = provider.GetRequiredService<IPlayerService>();
        var logger = provider.CreateLogger<GiveTitleToPlayerAction>();

        var taskList = new List<Task>();

        foreach (var playerId in context.PlayerIds)
        {
            taskList.Add(playerService.GrantPlayerTitleAsync(playerId, actionItem.Message));
        }

        await Task.WhenAll(taskList);

        logger.LogInformation(
            "Title '{Title}' granted to {Player}", actionItem.Message,
            string.Join(", ", context.PlayerIds)
        );

        return ScriptActionResult.Successful();
    }
}