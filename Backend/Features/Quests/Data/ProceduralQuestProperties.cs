using System.Collections.Generic;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public class ProceduralQuestProperties
{
    public IEnumerable<string> RewardTextList { get; set; } = [];
    public long QuantaReward { get; set; } = 0;
    public Dictionary<long, long> InfluenceReward { get; set; } = [];
    public Dictionary<string, long> ItemRewardMap { get; set; } = [];
}