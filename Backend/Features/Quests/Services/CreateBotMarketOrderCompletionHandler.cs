using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Interfaces;
using Mod.DynamicEncounters.Features.Market.Data;
using Mod.DynamicEncounters.Features.Market.Interfaces;
using Mod.DynamicEncounters.Features.Quests.Data;
using Mod.DynamicEncounters.Features.Quests.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Features.Quests.Services;

public class CreateBotMarketOrderCompletionHandler(QuestCompletionHandlerContext context) : IQuestTaskCompletionHandler
{
    public async Task<QuestTaskCompletionHandlerOutcome> HandleCompletion()
    {
        var playerQuestRepository = context.Provider.GetRequiredService<IPlayerQuestRepository>();
        var questItem = await playerQuestRepository.GetAsync(context.QuestTaskId.QuestId);

        if (questItem == null)
        {
            return QuestTaskCompletionHandlerOutcome.QuestTaskNotFound(context.QuestTaskId);
        }

        var taskItem = questItem.GetTaskOrNull(context.QuestTaskId);
        if (taskItem == null)
        {
            return QuestTaskCompletionHandlerOutcome.QuestTaskNotFound(context.QuestTaskId);
        }
        
        var featureService = context.Provider.GetRequiredService<IFeatureReaderService>();
        var bank = context.Provider.GetGameplayBank();
        var recipePriceCalculator = context.Provider.GetRequiredService<IRecipePriceCalculator>();
        var logger = context.Provider.CreateLogger<CreateBotMarketOrderCompletionHandler>();
        
        var questMarketOrderOwnerId = await featureService.GetIntValueAsync("QuestMarketOrderOwnerId", 16524);
        var questSafeMarketId = await featureService.GetIntValueAsync("QuestSafeMarketId", 20);
        var questMarketMargin = await featureService.GetDoubleValueAsync("QuestMarketMargin", 2);
        var questMarketPlayerEffectStrength = await featureService.GetDoubleValueAsync("QuestMarketPlayerEffectStrength", 10);
        var priceMap = await recipePriceCalculator.GetItemPriceMap();
        //OWNID = 16524
        //PLAYERID = 10095
        
        var marketOrderRepository = context.Provider.GetRequiredService<IMarketOrderRepository>();

        foreach (var item in taskItem.Definition.Items)
        {
            if (!priceMap.TryGetValue(item.ElementTypeName, out var recipeOutputData))
            {
                logger.LogWarning("Could not find price for {Item}", item.ElementTypeName);
                continue;
            }
            
            await marketOrderRepository.CreateMarketOrder(new MarketItem
            {
                OwnerId = (ulong)questMarketOrderOwnerId,
                Quantity = (long)(item.Quantity * questMarketPlayerEffectStrength),
                MarketId = (ulong)questSafeMarketId,
                ItemTypeId = bank.IdFor(item.ElementTypeName),
                Price = (long)(recipeOutputData.GetUnitPrice() * questMarketMargin)
            });
            
            logger.LogInformation("Market order created for {Item}", item.ElementTypeName);
        }

        return QuestTaskCompletionHandlerOutcome.Handled();
    }
}