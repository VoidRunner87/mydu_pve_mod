using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend;
using Backend.Business;
using Backend.Database;
using Backend.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Overrides.Actions.Data;
using Newtonsoft.Json;
using NQ;
using NQ.Interfaces;
using NQutils.Storage;
using Orleans;

namespace Mod.DynamicEncounters.Overrides.Actions;

public class GiveTakePlayerItemsAction(IServiceProvider provider)
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
        
        var transaction = await itemStorage.MakeTransaction(Tag.HttpCall("items"));
        
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

        await transaction.Commit();
    }
}