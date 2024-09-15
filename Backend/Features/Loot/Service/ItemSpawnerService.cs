﻿using System;
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

    public async Task SpawnFuel(SpawnFuelCommand command)
    {
        var spaceFuelContainers = new List<ElementId>();

        await foreach (var container in GetAvailableContainers<SpaceFuelContainer>(command.ConstructId))
        {
            spaceFuelContainers.Add(container);
        }

        var random = provider.GetRandomProvider().GetRandom();

        var itemDef = _bank.GetDefinition(command.FuelType);

        if (itemDef == null)
        {
            _logger.LogError("No item definition found for {Item}", command.FuelType);

            await _errorService.AddAsync(
                new ErrorItem(
                    "loot",
                    "failed_to_find_item_def",
                    new
                    {
                        command.FuelType,
                        command.ConstructId
                    }
                )
            );

            return;
        }

        var elementManagementGrain = _orleans.GetElementManagementGrain();
        var elementGrain = _orleans.GetConstructElementsGrain(command.ConstructId);

        foreach (var container in spaceFuelContainers)
        {
            var containerGrain = _orleans.GetContainerGrain(container);
            var containerInfo = await containerGrain.Get(ModBase.Bot.PlayerId);
            var elementInfo = await elementGrain.GetElement(container);

            var volume = random.NextInt64(0, (long)containerInfo.maxVolume);

            try
            {
                await _dataAccessor.SetDynamicProperty(
                    ElementPropertyUpdate.Create(
                        elementInfo,
                        "current_volume",
                        new PropertyValue(volume)
                    )
                );
                
                await _dataAccessor.ContainerGiveAsync(
                    (long)container.elementId,
                    new ItemAndQuantity
                    {
                        item = new ItemInfo
                        {
                            type = itemDef.Id
                        },
                        quantity = volume
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
                            command.FuelType,
                            command.ConstructId,
                            quantity = volume
                        }
                    )
                );
            }
        }

        _logger.LogInformation("Items Spawned on Construct {Construct}", command.ConstructId);
    }

    private async IAsyncEnumerable<ElementId> GetAvailableContainers<T>(ulong constructId) where T : ContainerUnit
    {
        var constructElementsGrain = _orleans.GetConstructElementsGrain(constructId);
        var containers = await constructElementsGrain.GetElementsOfType<SpaceFuelContainer>();

        foreach (var container in containers)
        {
            yield return container;
        }
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