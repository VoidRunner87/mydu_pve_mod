using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Overrides.ApiClient.Interfaces;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Overrides.ApiClient.Services;

public class WarpAnchorApiClient(IServiceProvider provider) : IWarpAnchorApiClient
{
    private readonly IHttpClientFactory _httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
    private readonly ILogger _logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger<WarpAnchorApiClient>();

    public async Task SetWarpEndCooldown(SetWarpEndCooldownRequest request)
    {
        var url = Path.Combine(PveModBaseUrl.GetBaseUrl(), "warp/cooldown");
        
        using var client = _httpClientFactory.CreateClient();

        var responseMessage = await client.PostAsync(new Uri(url),
            new StringContent(
                JsonConvert.SerializeObject(request),
                Encoding.UTF8,
                "application/json"
            ));

        var responseString = await responseMessage.Content.ReadAsStringAsync();

        _logger.LogInformation("SetWarpEndCooldown Response: {StatusCode}: {ResponseString}",
            responseMessage.StatusCode, responseString);
    }
}