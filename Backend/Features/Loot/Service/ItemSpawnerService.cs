using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend;
using Backend.Business;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Common.Data;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Loot.Data;
using Mod.DynamicEncounters.Features.Loot.Interfaces;
using Mod.DynamicEncounters.Helpers;
using Newtonsoft.Json;
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

    public async Task SpawnItems(SpawnItemOnRandomContainersCommand command)
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
            }
        }

        _logger.LogInformation("Items Spawned on Construct {Construct}", command.ConstructId);
    }

    public async Task SpawnItemsForPlayersAround(SpawnItemOnRandomContainersAroundAreaCommand command)
    {
        var areaScanService = provider.GetRequiredService<IAreaScanService>();
        var random = provider.GetRandomProvider().GetRandom();
        var contacts = (await areaScanService.ScanForPlayerContacts(
            command.InstigatorConstructId,
            command.Position,
            command.Radius,
            command.Limit)).ToHashSet();

        contacts = contacts.Where(x => x.ConstructId != command.InstigatorConstructId)
            .ToHashSet();

        if (contacts.Count == 0)
        {
            return;
        }

        var randomContact = random.PickOneAtRandom(contacts);

        await SpawnItems(new SpawnItemOnRandomContainersCommand(randomContact.ConstructId, command.ItemBag));
    }

    public async Task GiveTakeItemsWithCallback(GiveTakePlayerItemsWithCallbackCommand command)
    {
        if (command.Items.Any(x => !x.IsValid()))
        {
            throw new Exception("Invalid items to Give / Take");
        }
        
        var modManagerGrain = _orleans.GetModManagerGrain();
        var itemOperation = new ItemOperation
        {
            Items = command.Items.Select(x => new ItemOperation.ItemDefinition
            {
                Id = x.ElementId ?? 0,
                Name = x.ElementTypeName.Name,
                Quantity = x.Quantity,
            }),
            Owner = command.Owner,
            Properties = command.Properties,
            OnSuccessCallbackUrl = command.OnSuccessCallbackUrl,
            OnFailCallbackUrl = command.OnFailCallbackUrl
        };

        await modManagerGrain.TriggerModAction(
            command.PlayerId,
            new ModAction
            {
                actionId = 100, // TODO enum
                playerId = command.PlayerId,
                modName = "Mod.DynamicEncounters",
                payload = JsonConvert.SerializeObject(itemOperation)
            }
        );
    }

    public async Task SpawnSpaceFuel(SpawnFuelCommand command)
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

            return;
        }
        
        var fuelDef = _bank.GetBaseObject<Fuel>(itemDef.Id);
        
        if (fuelDef == null)
        {
            _logger.LogError("No fuel base obj found for {Item}", command.FuelType);

            return;
        }

        var modManagerGrain = _orleans.GetModManagerGrain();

        foreach (var container in spaceFuelContainers)
        {
            var containerGrain = _orleans.GetContainerGrain(container);
            var containerInfo = await containerGrain.Get(ModBase.Bot.PlayerId);

            var volume = random.NextInt64(0, (long)containerInfo.maxVolume);

            try
            {
                var itemOperation = new ItemOperation
                {
                    Items = [
                        new ItemOperation.ItemDefinition
                        {
                            Quantity = new LitreQuantity(volume).ToQuantity(),
                            Name = command.FuelType,
                        }
                    ],
                    BypassLock = true
                };

                await modManagerGrain.TriggerModAction(
                    ModBase.Bot.PlayerId,
                    new ActionBuilder()
                        .GiveTakeContainer()
                        .WithElementId(container)
                        .WithPayload(itemOperation)
                        .Build()
                );
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to fuel item to container");
            }
        }

        _logger.LogInformation("Fuel Spawn requested on Construct {Construct}", command.ConstructId);
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