using System;
using System.Linq;
using System.Threading.Tasks;
using BotLib.Generated;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Loot.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;
using NQutils.Def;
using Orleans;

namespace Mod.DynamicEncounters.Features.Loot.Service;

public class ElementReplacerService(IServiceProvider provider) : IElementReplacerService
{
    private readonly IClusterClient _orleans = provider.GetOrleans();
    private readonly ILogger<ElementReplacerService> _logger = provider.CreateLogger<ElementReplacerService>();

    public async Task ReplaceSingleElementAsync(ulong constructId, string elementTypeName, string withElementTypeName)
    {
        var bank = provider.GetGameplayBank();
        var elementDef = bank.GetDefinition(elementTypeName);

        if (elementDef == null)
        {
            _logger.LogError("Element Def NULL");
            return;
        }
        
        var replaceElDef = bank.GetDefinition(withElementTypeName);

        if (replaceElDef == null)
        {
            _logger.LogError("Replace Element Def NULL");
            return;
        }

        var constructElementsGrain = _orleans.GetConstructElementsGrain(constructId);
        var elementIds = await constructElementsGrain.GetElementsOfType<ConstructElement>();

        if (elementIds.Count == 0)
        {
            _logger.LogError("Element IDS COUNT = 0");
            return;
        }
        
        var element = (await Task.WhenAll(elementIds.Select(constructElementsGrain.GetElement)))
            .FirstOrDefault(x => x.elementType == elementDef.Id || bank.GetDefinition(x.elementType)!.IsChildOf(elementDef.Id));

        if (element == null)
        {
            _logger.LogError("Element is NULL");
            return;
        }

        var elementInfo = await constructElementsGrain.GetElement(element.elementId);
        var elPos = elementInfo.position;
        var elRot = elementInfo.rotation;

        await ModBase.Bot.Req.BotGiveItems(
            new ItemAndQuantityList
            {
                content =
                [
                    new()
                    {
                        item = new ItemInfo
                        {
                            type = replaceElDef.Id,
                        },
                        quantity = 1
                    }
                ]
            }
        );
        
        _logger.LogInformation("Added Item to the Bot Inventory");

        var inventory = await ModBase.Bot.Req.InventoryGet();
        var item = inventory.content
            .First(x => x.content.type == replaceElDef.Id);

        var playerId = await GetPlayerWithRightsOnConstruct(constructId);
        var elementManagementGrain = _orleans.GetElementManagementGrain();
        
        _logger.LogInformation("Construct Owner Player Id = {Id}", playerId);
        
        await ModBase.Bot.Req.ElementAdd(
            new ElementDeploy
            {
                element = new ElementInfo
                {
                    constructId = constructId,
                    elementType = replaceElDef.Id,
                    position = elPos,
                    rotation = elRot
                },
                fromInventory = new ItemId
                {
                    ownerId = item.content.owner,
                    instanceId = item.content.id,
                    typeId = item.content.type
                }
            }
        );
        
        _logger.LogInformation("Added Element");

        var elInConstruct = new ElementInConstruct
        {
            constructId = constructId,
            elementId = element.elementId
        };

        await elementManagementGrain.ElementDestroy(playerId, elInConstruct);
        
        _logger.LogInformation("Destroyed Replaced Element");
    }

    private async Task<ulong> GetPlayerWithRightsOnConstruct(ulong constructId)
    {
        var constructInfoGrain = _orleans.GetConstructInfoGrain(constructId);
        var constructInfo = await constructInfoGrain.Get();
        var ownerId = constructInfo.mutableData.ownerId;
        var playerId = ownerId.playerId;
        if (ownerId.IsOrg())
        {
            playerId = await _orleans.GetOrganizationGrain(ownerId.organizationId)
                .EffectiveSuperLegate();
        }

        return playerId;
    }
}