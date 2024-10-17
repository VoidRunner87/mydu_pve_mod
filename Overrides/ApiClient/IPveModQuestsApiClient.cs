using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Mod.DynamicEncounters.Overrides.ApiClient;

public interface IPveModQuestsApiClient
{
    Task<JToken> GetPlayerQuestsAsync(ulong playerId);
    Task<JToken> GetNpcQuests(ulong playerId, long factionId, Guid territoryId, int seed);
    Task<BasicOutcome> AcceptQuest(Guid questId, ulong playerId, long factionId, Guid territoryId, int seed);
}

