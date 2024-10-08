using System;
using System.Threading.Tasks;
using BotLib.Generated;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.NQ.Interfaces;
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
    
    public string Name => ActionName;

    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var provider = context.ServiceProvider;
        
        var logger = provider.CreateLogger<SendDirectMessageAction>();
        var constructService = provider.GetRequiredService<IConstructService>();
        // var gameAlertService = provider.GetRequiredService<IGameAlertService>();
        
        var constructCode = "?";
        var constructName = "???";

        if (context.ConstructId.HasValue)
        {
            constructCode = $"{context.ConstructId}";
            constructCode = constructCode[^3..];
            
            var constructInfo = await constructService.GetConstructInfoAsync(context.ConstructId.Value);
            if (constructInfo != null)
            {
                constructName = constructInfo.rData.name;
            }
        }

        foreach (var playerId in context.PlayerIds)
        {
            try
            {
                var message = $"[{constructCode}] {constructName}: {actionItem.Message}";
                // await gameAlertService.PushErrorAlert(playerId, $"[{constructCode}] {constructName}: {actionItem.Message}");
                
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
            catch (BusinessException e)
            {
                logger.LogError(e, "Failed to Send Chat Message. Reconnecting Bot");

                try
                {
                    await ModBase.Bot.Reconnect();
                }
                catch (Exception e2)
                {
                    logger.LogError(e2, "Failed to Reconnect");
                }
            }
        }
        
        logger.Debug("DM Messages Sent");

        return ScriptActionResult.Successful();
    }

    public string GetKey() => Name;
}