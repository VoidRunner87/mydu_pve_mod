using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.NQ.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

public class GiveTitleToPlayerAction(ScriptActionItem actionItem) : IScriptAction
{
    public string Name { get; } = Guid.NewGuid().ToString();

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