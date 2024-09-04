using System;
using System.Threading.Tasks;
using BotLib.Generated;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Helpers;
using NQ;
using Orleans.Runtime;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

public class ChatDmScriptAction(string message) : IScriptAction
{
    public string Name { get; } = Guid.NewGuid().ToString();

    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var logger = context.ServiceProvider
            .CreateLogger<ChatDmScriptAction>();

        foreach (var playerId in context.PlayerIds)
        {
            await ModBase.Bot.Req.ChatMessageSend(
                new MessageContent
                {
                    channel = new MessageChannel
                    {
                        channel = MessageChannelType.PRIVATE,
                        targetId = playerId
                    },
                    message = message
                }
            );
        }
        
        logger.Debug("DM Messages Sent");

        return ScriptActionResult.Successful();
    }

    public string GetKey() => Name;
}