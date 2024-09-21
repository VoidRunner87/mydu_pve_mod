using System;
using System.Linq;
using System.Threading.Tasks;
using Backend;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Common;
using Mod.DynamicEncounters.Features.Loot.Data;
using Mod.DynamicEncounters.Features.Loot.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQutils.Def;

namespace Mod.DynamicEncounters.Features.Loot.Service;

public class LootGeneratorService(IServiceProvider provider) : ILootGeneratorService
{
    private readonly IRandomProvider _randomProvider = provider.GetRandomProvider();
    private readonly ILootDefinitionRepository _repository = provider.GetRequiredService<ILootDefinitionRepository>();
    private readonly IGameplayBank _bank = provider.GetGameplayBank();
    private readonly ILogger<LootGeneratorService> _logger = provider.CreateLogger<LootGeneratorService>();
    private readonly IElementReplacerService _replacerService = provider.GetRequiredService<IElementReplacerService>();

    public async Task<ItemBagData> GenerateAsync(LootGenerationArgs args)
    {
        var random = _randomProvider.GetRandom();

        var lootDefinitionItems = (await _repository.GetAllActiveByTagsAsync(args.Tags))
            .ToArray();

        random.Shuffle(lootDefinitionItems);

        var itemBag = new ItemBagData(args.MaxBudget);

        var allItemRules = lootDefinitionItems
            .SelectMany(x => x.ItemRules)
            .ToArray();

        random.Shuffle(allItemRules);

        foreach (var itemRule in allItemRules)
        {
            itemRule.Sanitize();

            var itemDef = _bank.GetDefinition(itemRule.ItemName);

            if (itemDef == null)
            {
                _logger.LogError("Could not find Item def for {Item}", itemRule.ItemName);
                continue;
            }

            var roll100 = random.NextDouble();
            var itemMinChanceRoll = 1 - itemRule.Chance;
            
            if (roll100 < itemMinChanceRoll)
            {
                continue;
            }

            var isMineAbleItem = itemDef.Is<MineableMaterial>();

            var randomQuantity = random.NextInt64(itemRule.MinQuantity, itemRule.MaxQuantity);
            var randomCost = random.NextInt64(itemRule.MinSpawnCost, itemRule.MaxSpawnCost);

            IQuantity quantity = isMineAbleItem
                ? new OreQuantity(randomQuantity)
                : new DefaultQuantity(randomQuantity);

            var stillWithinBudget = itemBag.AddEntry(
                randomCost,
                new ItemBagData.ItemAndQuantity(itemRule.ItemName, quantity)
            );

            if (!stillWithinBudget)
            {
                return itemBag;
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
        }
        
        return itemBag;
    }
}