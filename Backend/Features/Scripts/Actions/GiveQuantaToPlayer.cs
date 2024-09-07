using System;
using System.Threading.Tasks;
using Backend;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Services;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

[ScriptActionName(ActionName)]
public class GiveQuantaToPlayer(ScriptActionItem actionItem) : IScriptAction
{
    public const string ActionName = "give-quanta";
    public string Name { get; } = Guid.NewGuid().ToString();

    public string GetKey() => Name;
    
    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var provider = context.ServiceProvider;
        var orleans = provider.GetOrleans();

        if (context.PlayerIds.Count == 0)
        {
            return ScriptActionResult.Failed();
        }
        
        var valuePerPlayer = actionItem.Value / context.PlayerIds.Count;
        
        foreach (var playerId in context.PlayerIds)
        {
            var notificationGrain = orleans.GetNotificationGrain(playerId);
            var transfer = new WalletTransfer
            {
                amount = (ulong)valuePerPlayer,
                reason = actionItem.Message,
                fromWallet = new EntityId
                {
                    playerId = 2
                },
                toWallet = new EntityId
                {
                    playerId = playerId
                }
            };
            
            await notificationGrain.AddNewNotification(
                Notifications.WalletReceivedMoney(transfer.fromWallet, transfer.amount)
            );
        }

        await Task.Yield(); // TODO
        
        return ScriptActionResult.Failed();
    }
}