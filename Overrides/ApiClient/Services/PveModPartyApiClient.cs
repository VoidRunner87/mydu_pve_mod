using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Overrides.ApiClient.Data;
using Mod.DynamicEncounters.Overrides.ApiClient.Interfaces;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Overrides.ApiClient.Services;

public class PveModPartyApiClient(IServiceProvider provider) : IPveModPartyApiClient
{
    private readonly IHttpClientFactory _httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
    
    public async Task<IEnumerable<PlayerPartyItem>> GetPartyByPlayerId(ulong playerId, CancellationToken cancellationToken)
    {
        var url = Path.Combine(PveModBaseUrl.GetBaseUrl(), "party", $"{playerId}");
        
        using var client = _httpClientFactory.CreateClient();

        var responseMessage = await client.GetAsync(
            url,
            cancellationToken
        );

        var jsonString = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

        return JsonConvert.DeserializeObject<IEnumerable<PlayerPartyItem>>(jsonString);
    }
}