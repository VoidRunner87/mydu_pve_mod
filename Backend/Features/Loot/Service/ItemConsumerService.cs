using System;
using System.Threading.Tasks;
using Backend;
using Backend.Business;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Loot.Data;
using Mod.DynamicEncounters.Features.Loot.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQutils.Sql;
using Orleans;

namespace Mod.DynamicEncounters.Features.Loot.Service;

public class ItemConsumerService(IServiceProvider provider) : IItemConsumerService
{
    private readonly IClusterClient _orleans = provider.GetOrleans();
    private readonly IGameplayBank _bank = provider.GetGameplayBank();
    private readonly IDataAccessor _dataAccessor = provider.GetRequiredService<IDataAccessor>();
    private readonly ILogger<ItemConsumerService> _logger = provider.CreateLogger<ItemConsumerService>();
    
    public async Task ConsumeItems(ConsumeItemsOnPlayerInventoryCommand command)
    {
        foreach (var entry in command.Items)
        {
            var itemName = entry.ElementTypeName.Name;
            var itemDef = _bank.GetDefinition(itemName);

            if (itemDef == null)
            {
                _logger.LogError("No item definition found for {Item}", itemName);

                continue;
            }

            try
            {
                await Task.Yield();
                // await _dataAccessor.PlayerInventoryGiveAsync(
                //     command.PlayerId,
                //     new ItemAndQuantity
                //     {
                //         item = new ItemInfo
                //         {
                //             type = itemDef.Id
                //         },
                //         quantity = entry.Quantity
                //     }
                // );
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to consume item {entry.ElementTypeName.Name}");
            }
        }

        _logger.LogInformation("Items Consumed from Player {PlayerId}", command.PlayerId);
    }
}