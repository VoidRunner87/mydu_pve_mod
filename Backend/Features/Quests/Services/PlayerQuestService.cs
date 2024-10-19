using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Quests.Data;
using Mod.DynamicEncounters.Features.Quests.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using NQ;

namespace Mod.DynamicEncounters.Features.Quests.Services;

public class PlayerQuestService(IServiceProvider provider) : IPlayerQuestService
{
    private readonly IPlayerQuestRepository _repository = provider.GetRequiredService<IPlayerQuestRepository>();

    public async Task<PlayerAcceptQuestOutcome> AcceptQuestAsync(
        PlayerId playerId,
        ProceduralQuestItem proceduralQuestItem
    )
    {
        var playerQuestItems = (await _repository.GetAllAsync(playerId)).ToList();

        if (playerQuestItems.Any(x => x.OriginalQuestId == proceduralQuestItem.Id))
        {
            return PlayerAcceptQuestOutcome.AlreadyAccepted();
        }

        if (playerQuestItems.Count >= 10)
        {
            return PlayerAcceptQuestOutcome.MaxNumberOfActiveQuestsReached();
        }

        var props = proceduralQuestItem.Properties;

        var item = new PlayerQuestItem(
            Guid.NewGuid(),
            proceduralQuestItem.Id,
            proceduralQuestItem.FactionId,
            playerId,
            proceduralQuestItem.Type,
            QuestStatus.InProgress,
            proceduralQuestItem.Seed,
            new PlayerQuestItem.QuestProperties(
                proceduralQuestItem.Title,
                string.Empty
            )
            {
                RewardTextList = props.RewardTextList,
                InfluenceReward = props.InfluenceReward,
                QuantaReward = props.QuantaReward,
                ItemRewardMap = props.ItemRewardMap
            },
            DateTime.UtcNow,
            DateTime.UtcNow + TimeSpan.FromHours(3),
            proceduralQuestItem.TaskItems.ToList(),
            new ScriptActionItem(),
            new ScriptActionItem()
        );

        await _repository.AddAsync(item);

        return PlayerAcceptQuestOutcome.Accepted();
    }

    public async Task<PlayerAbandonQuestOutcome> AbandonQuestAsync(PlayerId playerId, Guid questId)
    {
        await _repository.DeleteAsync(playerId, questId);

        return PlayerAbandonQuestOutcome.Abandoned();
    }
}