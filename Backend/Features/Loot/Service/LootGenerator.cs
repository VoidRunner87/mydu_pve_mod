using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Loot.Data;
using Mod.DynamicEncounters.Features.Loot.Extensions;
using Mod.DynamicEncounters.Features.Loot.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQutils.Def;

namespace Mod.DynamicEncounters.Features.Loot.Service;

public class LootGeneratorService(IServiceProvider provider) : ILootGeneratorService
{
    private readonly ILootDefinitionRepository _repository = provider.GetRequiredService<ILootDefinitionRepository>();
    private readonly IGameplayBank _bank = provider.GetGameplayBank();
    private readonly ILogger<LootGeneratorService> _logger = provider.CreateLogger<LootGeneratorService>();

    public async Task<ItemBagData> GenerateAsync(LootGenerationArgs args)
    {
        var random = new Random(args.Seed);

        var lootDefinitionItems = (await _repository.GetAllActiveTagsAsync(args.Operator, args.Tags))
            .ToArray();

        random.Shuffle(lootDefinitionItems);

        if (lootDefinitionItems.Length == 0)
        {
            return new ItemBagData
            {
                MaxBudget = args.MaxBudget,
                Tags = [],
                Name = string.Empty
            };
        }

        var randomLootDefItem = random.PickOneAtRandom(lootDefinitionItems);
        var itemBag = new ItemBagData
        {
            MaxBudget = args.MaxBudget,
            Tags = randomLootDefItem.ExtraTags,
            Name = randomLootDefItem.Name
        };

        var allItemRules = lootDefinitionItems
            .SelectMany(x => x.ItemRules)
            .ToArray();

        random.Shuffle(allItemRules);

        foreach (var itemRule in allItemRules)
        {
            itemRule.Sanitize();

            var randomQuantity = random.NextInt64(itemRule.MinQuantity, itemRule.MaxQuantity);
            var randomCost = random.NextInt64(itemRule.MinSpawnCost, itemRule.MaxSpawnCost);
            
            var itemDef = _bank.GetDefinition(itemRule.ItemName);

            var item = new BaseItem();

            if (itemDef != null)
            {
                item = _bank.GetBaseObject<BaseItem>(itemDef.ItemType());
                if (item == null)
                {
                    _logger.LogError("Could not find BaseObject for {Item}", itemRule.ItemName);
                    item = new BaseItem();
                }
            }

            var roll100 = random.NextDouble();
            var itemMinChanceRoll = 1 - itemRule.Chance;

            if (roll100 < itemMinChanceRoll)
            {
                continue;
            }

            var quantity = item.GetQuantityForElement(randomQuantity);

            var stillWithinBudget = itemBag.AddEntry(
                randomCost,
                new ItemBagData.ItemAndQuantity(itemRule.ItemName, quantity)
            );

            if (!stillWithinBudget)
            {
                break;
            }
        }

        var allElementRules = lootDefinitionItems
            .SelectMany(x => x.ElementRules)
            .Where(x => x.IsValid())
            .ToArray();

        foreach (var elementRule in allElementRules)
        {
            elementRule.Sanitize();

            var roll100 = random.NextDouble();
            var itemMinChanceRoll = 1 - elementRule.Chance;

            if (roll100 < itemMinChanceRoll)
            {
                continue;
            }

            var randomQuantity = random.NextInt64(elementRule.MinQuantity, elementRule.MaxQuantity);

            itemBag.ElementsToReplace.Add(
                new ItemBagData.ElementReplace(
                    elementRule.FindElement,
                    elementRule.ReplaceElement,
                    randomQuantity
                )
            );

            _logger.LogInformation("Replacing {Quantity}x {Item} with {Replacement}", randomQuantity,
                elementRule.FindElement, elementRule.ReplaceElement);
        }

        return itemBag;
    }

    public async Task<Dictionary<string, ItemBagData>> GenerateGrouped(LootGenerationArgs args)
    {
        var result = new Dictionary<string, ItemBagData>();

        var random = new Random(args.Seed);

        var lootDefinitionItems = (await _repository.GetAllActiveTagsAsync(args.Operator, args.Tags))
            .ToArray();

        random.Shuffle(lootDefinitionItems);

        if (lootDefinitionItems.Length == 0)
        {
            return result;
        }

        var randomLootDefItem = random.PickOneAtRandom(lootDefinitionItems);

        foreach (var definitionItem in lootDefinitionItems)
        {
            var itemBag = new ItemBagData
            {
                MaxBudget = args.MaxBudget,
                Name = randomLootDefItem.Name,
                Tags = definitionItem.ExtraTags
            };

            foreach (var itemRule in definitionItem.ItemRules)
            {
                itemRule.Sanitize();

                var itemDef = _bank.GetDefinition(itemRule.ItemName);

                if (itemDef == null)
                {
                    _logger.LogError("Could not find Item def for {Item}", itemRule.ItemName);
                    continue;
                }

                var item = _bank.GetBaseObject<BaseItem>(itemDef.ItemType());
                if (item == null)
                {
                    _logger.LogError("Could not find BaseObject for {Item}", itemRule.ItemName);
                    continue;
                }

                var roll100 = random.NextDouble();
                var itemMinChanceRoll = 1 - itemRule.Chance;

                if (roll100 < itemMinChanceRoll)
                {
                    continue;
                }

                var randomQuantity = random.NextInt64(itemRule.MinQuantity, itemRule.MaxQuantity);
                var randomCost = random.NextInt64(itemRule.MinSpawnCost, itemRule.MaxSpawnCost);

                var quantity = item.GetQuantityForElement(randomQuantity);

                var stillWithinBudget = itemBag.AddEntry(
                    randomCost,
                    new ItemBagData.ItemAndQuantity(itemRule.ItemName, quantity)
                );

                if (!stillWithinBudget)
                {
                    break;
                }
            }

            foreach (var elementRule in definitionItem.ElementRules)
            {
                elementRule.Sanitize();

                var roll100 = random.NextDouble();
                var itemMinChanceRoll = 1 - elementRule.Chance;

                if (roll100 < itemMinChanceRoll)
                {
                    continue;
                }

                var randomQuantity = random.NextInt64(elementRule.MinQuantity, elementRule.MaxQuantity);

                itemBag.ElementsToReplace.Add(
                    new ItemBagData.ElementReplace(
                        elementRule.FindElement,
                        elementRule.ReplaceElement,
                        randomQuantity
                    )
                );

                _logger.LogInformation("Replacing {Quantity}x {Item} with {Replacement}", randomQuantity,
                    elementRule.FindElement, elementRule.ReplaceElement);
            }

            result.Add(definitionItem.Name, itemBag);
        }

        return result;
    }
}