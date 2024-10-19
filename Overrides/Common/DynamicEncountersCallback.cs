using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mod.DynamicEncounters.Overrides.Common;

public static class DynamicEncountersCallback
{
    private const string PveModPlaceholder = "@{PVE_MOD}";
    
    public static async Task ExecuteCallback(IServiceProvider provider, string url)
    {
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DynamicEncountersCallback));
        
        url = url.Replace(PveModPlaceholder, Config.GetPveModBaseUrl());
        
        try
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            using var httpClient = httpClientFactory.CreateClient();

            var response = await httpClient.PostAsync(url, new StringContent(string.Empty));

            logger.LogInformation("Sent callback({Status}): {Url}", response.StatusCode, url);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to send callback to: {url}", url);
        }
    }
}