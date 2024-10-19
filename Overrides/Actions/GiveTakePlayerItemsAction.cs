using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Backend;
using Backend.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Overrides.Actions.Data;
using Mod.DynamicEncounters.Overrides.Common;
using Newtonsoft.Json;
using NQ;
using NQ.Interfaces;
using NQutils.Exceptions;
using NQutils.Storage;
using Orleans;
using ErrorCode = NQ.ErrorCode;

namespace Mod.DynamicEncounters.Overrides.Actions;

public class GiveTakePlayerItemsAction(IServiceProvider provider) : IModActionHandler
{
    public async Task HandleAction(ulong playerId, ModAction action)
    {
        var itemStorage = provider.GetRequiredService<IItemStorageService>();
        var orleans = provider.GetRequiredService<IClusterClient>();
        var bank = provider.GetRequiredService<IGameplayBank>();
        var playerInventoryGrain = orleans.GetPlayerInventoryGrain(playerId);
        var logger = provider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<GiveTakePlayerItemsAction>();

        var itemOperation = JsonConvert.DeserializeObject<ItemOperation>(action.payload);
        var callback = JsonConvert.DeserializeObject<CallbackData>(action.payload);

        var transaction = await itemStorage.MakeTransaction(Tag.HttpCall("items"));
        
        try
        {
            var itemQuantList = new List<ItemAndQuantity>();
            foreach (var item in itemOperation.Items)
            {
                var def = bank.GetDefinition(item.Name);
                if (def == null)
                {
                    logger.LogError("Definition for {Item} is NULL", item.Name);
                    continue;
                }

                var itemType = def.ItemType().itemType;

                itemQuantList.Add(
                    new ItemAndQuantity
                    {
                        quantity = item.Quantity,
                        item = new ItemInfo
                        {
                            type = itemType,
                            properties = itemOperation.Properties
                        }
                    }
                );

                logger.LogInformation("Found Item {Name} = {ItemType}", item.Name, itemType);
            }

            await playerInventoryGrain.GiveOrTakeItems(
                transaction,
                itemQuantList,
                new OperationOptions
                {
                    AllowPartial = false,
                    Reason = StorageReserveReason.RESERVE_DEPLOY,
                    User = new EntityId { playerId = playerId },
                    Requester = playerId,
                    BypassLock = true
                }
            );

            await Notifications.SimpleNotificationToPlayer(provider, playerId, "Inventory operation successful");
            await DynamicEncountersCallback.ExecuteCallback(provider, callback.OnSuccessCallbackUrl);
            
            await transaction.Commit();
        }
        catch (Exception e)
        {
            await transaction.Rollback();
            
            if (e is BusinessException bex)
            {
                switch (bex.error.code)
                {
                    case ErrorCode.InventoryFull:
                    case ErrorCode.InventoryNotEnough:
                    case ErrorCode.InventoryOperationError:
                    case ErrorCode.InventoryOverVolume:
                    case ErrorCode.InventoryNoSuchContainer:
                    case ErrorCode.InventoryInvalidItemType:
                    case ErrorCode.InventoryNotEmptySlot:
                        await Notifications.ErrorNotification(provider, playerId, "Failed. Check if your inventory is full");
                        break;
                }    
            }
            else
            {
                await Notifications.ErrorNotification(provider, playerId, "Unknown Failure. Report to the admins this error");
            }
            
            logger.LogError(e, "Failed to Give or Take Items for {Player}. Action: {Action}. Payload: {Payload}", playerId, action, action.payload);

            await DynamicEncountersCallback.ExecuteCallback(provider, callback.OnFailCallbackUrl);
        }
    }
}