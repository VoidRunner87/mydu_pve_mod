using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Overrides.ApiClient.Data;
using Mod.DynamicEncounters.Overrides.ApiClient.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mod.DynamicEncounters.Overrides.ApiClient.Services;

public class PveModQuestsApiClient(IServiceProvider provider) : IPveModQuestsApiClient
{
    private readonly IHttpClientFactory _httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();

    public async Task<JToken> GetPlayerQuestsAsync(ulong playerId)
    {
        var url = Path.Combine(PveModBaseUrl.GetBaseUrl(), "quest/player", $"{playerId}");

        using var client = _httpClientFactory.CreateClient();
        
        var responseMessage = await client.GetAsync(new Uri(url));

        return JToken.Parse(await responseMessage.Content.ReadAsStringAsync());
    }

    public async Task<JToken> GetNpcQuests(ulong playerId, long factionId, Guid territoryId, int seed)
    {
        var url = Path.Combine(PveModBaseUrl.GetBaseUrl(), "quest/giver");
        
        using var client = _httpClientFactory.CreateClient();

        var responseMessage = await client.PostAsync(
            new Uri(url),
            new StringContent(
                JsonConvert.SerializeObject(new
                {
                    playerId,
                    factionId,
                    territoryId,
                    seed
                }),
                Encoding.UTF8,
                "application/json"
            )
        );
        
        return JToken.Parse(await responseMessage.Content.ReadAsStringAsync());
    }

    public async Task<BasicOutcome> AcceptQuest(Guid questId, ulong playerId, long factionId, Guid territoryId, int seed)
    {
        var url = Path.Combine(PveModBaseUrl.GetBaseUrl(), "quest/player/accept");
        
        using var client = _httpClientFactory.CreateClient();

        var responseMessage = await client.PostAsync(
            new Uri(url),
            new StringContent(
                JsonConvert.SerializeObject(new
                {
                    questId,
                    playerId,
                    factionId,
                    territoryId,
                    seed
                }),
                Encoding.UTF8,
                "application/json"
            )
        );
        
        return JsonConvert.DeserializeObject<BasicOutcome>(await responseMessage.Content.ReadAsStringAsync());
    }

    public async Task<BasicOutcome> AbandonQuest(Guid questId, ulong playerId)
    {
        var url = Path.Combine(PveModBaseUrl.GetBaseUrl(), "quest/player/abandon");
        
        using var client = _httpClientFactory.CreateClient();

        var responseMessage = await client.PostAsync(
            new Uri(url),
            new StringContent(
                JsonConvert.SerializeObject(new
                {
                    questId,
                    playerId
                }),
                Encoding.UTF8,
                "application/json"
            )
        );
        
        return JsonConvert.DeserializeObject<BasicOutcome>(await responseMessage.Content.ReadAsStringAsync());
    }
}