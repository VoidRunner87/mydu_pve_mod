using System;
using System.Threading.Tasks;
using BotLib.Generated;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Services;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQutils.Exceptions;
using Orleans.Runtime;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

[ScriptActionName(ActionName)]
public class SendDirectMessageAction(ScriptActionItem actionItem) : IScriptAction
{
    public const string ActionName = "message";
    
    public string Name { get; } = Guid.NewGuid().ToString();

    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var logger = context.ServiceProvider
            .CreateLogger<SendDirectMessageAction>();

        foreach (var playerId in context.PlayerIds)
        {
            try
            {
                await ModBase.Bot.Req.ChatMessageSend(
                    new MessageContent
                    {
                        channel = new MessageChannel
                        {
                            channel = MessageChannelType.PRIVATE,
                            targetId = playerId
                        },
                        message = actionItem.Message
                    }
                );
            }
            catch (BusinessException e)
            {
                logger.LogError(e, "Failed to Send Chat Message. Reconnecting Bot");

                await ModBase.Bot.Reconnect();
            }
        }
        
        logger.Debug("DM Messages Sent");

        return ScriptActionResult.Successful();
    }

    public string GetKey() => Name;
}