using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Overrides.ApiClient.Interfaces;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Overrides.ApiClient.Services;

public class BehaviorContextApiClient(IServiceProvider provider) : IBehaviorContextApiClient
{
    private readonly IHttpClientFactory _httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();

    private readonly ILogger<BehaviorContextApiClient> _logger = provider.GetRequiredService<ILoggerFactory>()
        .CreateLogger<BehaviorContextApiClient>();
    
    public async Task RegisterDamage(RegisterDamageRequest request, CancellationToken cancellationToken = default)
    {
        var url = new Uri(new Uri(PveModBaseUrl.GetBaseUrl()), $"behavior/context/{request.ConstructId}/register-damage");
        
        using var client = _httpClientFactory.CreateClient();

        try
        {
            await client.PostAsync(
                url,
                new StringContent(
                    JsonConvert.SerializeObject(request),
                    Encoding.UTF8,
                    "application/json"
                ),
                cancellationToken
            );
            
            _logger.LogInformation("Register Damage Done for Construct {ConstructId} - {Damage}", request.ConstructId, request.Damage);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to Register Damage on Construct {ConstructId}", request.ConstructId);
        }
    }
}