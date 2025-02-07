using System;
using System.Threading.Tasks;
using Backend.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Common.Helpers;
using Mod.DynamicEncounters.Features.Events.Data;
using Mod.DynamicEncounters.Features.Events.Interfaces;
using Mod.DynamicEncounters.Features.NQ.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Services;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;
using NQutils.Sql;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

[ScriptActionName(ActionName, Description = Description)]
public class GiveQuantaToPlayer(ScriptActionItem actionItem) : IScriptAction
{
    public const string ActionName = "give-quanta";
    public const string Description = "Gives an amount of quanta to all players in the context of the execution";
    public string Name => ActionName;

    public string GetKey() => Name;

    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var provider = context.ServiceProvider;
        var logger = provider.CreateLogger<GiveQuantaToPlayer>();
        var orleans = provider.GetOrleans();
        var sql = provider.GetRequiredService<ISql>();
        var walletService = provider.GetRequiredService<IWalletService>();
        var eventService = provider.GetRequiredService<IEventService>();

        if (context.PlayerIds.Count == 0)
        {
            return ScriptActionResult.Failed();
        }

        var valuePerPlayer = actionItem.Value / context.PlayerIds.Count;

        foreach (var playerId in context.PlayerIds)
        {
            try
            {
                await walletService.AddToPlayerWallet(
                    playerId,
                    (ulong)valuePerPlayer
                );

                await eventService.PublishAsync(
                    new QuantaGivenToPlayerEvent(
                        playerId,
                        context.Sector,
                        context.ConstructId,
                        context.PlayerIds.Count,
                        (ulong)valuePerPlayer
                    )
                );
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to send {Q:N2}h quanta to player {Player}", valuePerPlayer, playerId);
            }

            try
            {
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

                await sql.InsertWalletOperation(
                    transfer.toWallet,
                    transfer.fromWallet,
                    (long)transfer.amount,
                    WalletOperationType.Reward,
                    new WalletOperationDetail
                    {
                        transfer = new WalletOperationTransfer
                        {
                            reason = actionItem.Message,
                            initiatingPlayer = new NamedEntity
                            {
                                id = transfer.toWallet,
                                name = "United Earth Defense Force"
                            }
                        }
                    }).OnError(exception => { logger.LogError(exception, "Failed to Insert Wallet Operation"); });

                var notificationGrain = orleans.GetNotificationGrain(playerId);
                await notificationGrain.AddNewNotification(
                    Notifications.WalletReceivedMoney(transfer.fromWallet, transfer.amount)
                ).OnError(exception =>
                {
                    logger.LogError(exception, "Failed to send notification for wallet operation");
                });
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to Execute Quanta Transfer Auxiliary operations for Player {Player}",
                    playerId);
            }
        }

        return ScriptActionResult.Successful();
    }
}