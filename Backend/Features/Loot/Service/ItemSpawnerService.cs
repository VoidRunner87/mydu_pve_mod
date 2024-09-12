using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend;
using Backend.Business;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Loot.Data;
using Mod.DynamicEncounters.Features.Loot.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;
using NQutils.Def;
using Orleans;

namespace Mod.DynamicEncounters.Features.Loot.Service;

public class ItemSpawnerService(IServiceProvider provider) : IItemSpawnerService
{
    private readonly IClusterClient _orleans = provider.GetOrleans();
    private readonly IGameplayBank _bank = provider.GetGameplayBank();
    private readonly IDataAccessor _dataAccessor = provider.GetRequiredService<IDataAccessor>();
    private readonly ILogger<ItemSpawnerService> _logger = provider.CreateLogger<ItemSpawnerService>();
    private readonly IErrorService _errorService = provider.GetRequiredService<IErrorService>();
    
    public async Task SpawnItems(SpawnItemCommand command)
    {
        var containers = new List<ElementId>();

        await foreach (var container in GetAvailableContainers(command.ConstructId))
        {
            containers.Add(container);
        }

        if (containers.Count == 0)
        {
            _logger.LogWarning("No containers found on Construct {Construct}", command.ConstructId);
            
            return;
        }

        var random = provider.GetRandomProvider().GetRandom();
        
        foreach (var entry in command.ItemBag.GetEntries())
        {
            var targetContainer = random.PickOneAtRandom(containers);

            var itemDef = _bank.GetDefinition(entry.ItemName);

            if (itemDef == null)
            {
                _logger.LogError("No item definition found for {Item}", entry.ItemName);

                await _errorService.AddAsync(
                    new ErrorItem(
                        "loot",
                        "failed_to_find_item_def",
                        new
                        {
                            entry.ItemName,
                            command.ConstructId
                        }
                    )
                );
                
                continue;
            }

            try
            {
                await _dataAccessor.ContainerGiveAsync(
                    (long)targetContainer.elementId,
                    new ItemAndQuantity
                    {
                        item = new ItemInfo
                        {
                            type = itemDef.Id
                        },
                        quantity = entry.Quantity.ToQuantity()
                    }
                );
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to add item to container");
                
                await _errorService.AddAsync(
                    new ErrorItem(
                        "loot",
                        "failed_to_add_to_container",
                        new
                        {
                            entry.ItemName,
                            command.ConstructId,
                            quantity = entry.Quantity.ToQuantity()
                        }
                    )
                );
            }
            
        }
        
        _logger.LogInformation("Items Spawned on Construct {Construct}", command.ConstructId);
    }

    private async IAsyncEnumerable<ElementId> GetAvailableContainers(ulong constructId)
    {
        var constructElementsGrain = _orleans.GetConstructElementsGrain(constructId);
        var containerHubs = await constructElementsGrain.GetElementsOfType<ContainerHub>();

        if (containerHubs.Count != 0)
        {
            var firstContainerHub = containerHubs.First();
            var containerGrain = _orleans.GetContainerGrain(firstContainerHub);
            var masterContainer = await containerGrain.MasterContainer();

            if (masterContainer == null)
            {
                yield return firstContainerHub;
            }
            else
            {
                yield return masterContainer.Value;
            }
        }
        else
        {
            var containers = await constructElementsGrain.GetElementsOfType<ItemContainer>();

            foreach (var container in containers)
            {
                yield return container;
            }
        }
    }
}