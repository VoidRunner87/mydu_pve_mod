using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Common.Helpers;
using Mod.DynamicEncounters.Features.VoxelService.Data;
using Mod.DynamicEncounters.Features.VoxelService.Interfaces;
using Mod.DynamicEncounters.Helpers;
using Newtonsoft.Json;
using NQ;

namespace Mod.DynamicEncounters.Features.VoxelService.Services;

public class VoxelServiceClient(IServiceProvider provider) : IVoxelServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
    private const string UrlEnvironmentVariable = "PVE_VOXEL_SERVICE_URL";
    private const string EnabledEnvironmentVariable = "PVE_VOXEL_SERVICE_ENABLED";

    private readonly ILogger _logger = provider.CreateLogger<VoxelServiceClient>();
    
    public bool IsEnabled()
    {
        return !string.IsNullOrEmpty(
            EnvironmentVariableHelper.GetEnvironmentVarOrDefault(EnabledEnvironmentVariable, "")
        );
    }

    public async Task TriggerConstructCacheAsync(ConstructId constructId)
    {
        if (!IsEnabled())
        {
            return;
        }

        try
        {
            var httpClient = _httpClientFactory.CreateClient();

            await httpClient.PostAsync(
                new Uri(
                    new Uri(GetPveVoxelServiceBaseUrl()),
                    $"/v1/mesh/cache/{constructId.constructId}"
                ),
                null
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to Trigger Construct Cache");
        }
    }

    public async Task<QueryRandomPointOutcome> QueryRandomPoint(QueryRandomPoint query)
    {
        if (!IsEnabled())
        {
            return QueryRandomPointOutcome.Disabled();
        }

        try
        {
            var httpClient = _httpClientFactory.CreateClient();

            var cts = new CancellationTokenSource();
            cts.CancelAfter(300);
            
            var response = await httpClient.PostAsync(
                new Uri(
                    new Uri(GetPveVoxelServiceBaseUrl()),
                    $"/v1/mesh/cache/{query.ConstructId.constructId}/random-point"
                ),
                new StringContent(
                    JsonConvert.SerializeObject(
                        new
                        {
                            FromPosition = query.FromLocalPosition
                        }
                    ),
                    Encoding.UTF8,
                    "application/json"
                ),
                cts.Token
            );

            if (!response.IsSuccessStatusCode)
            {
                return QueryRandomPointOutcome.Failed(response);
            }

            var point = await response.Content.ReadFromJsonAsync<Vec3>();

            return QueryRandomPointOutcome.FoundPosition(point);
        }
        catch (Exception e)
        {
            return QueryRandomPointOutcome.Failed(e);
        }
    }

    private static string GetPveVoxelServiceBaseUrl()
        => EnvironmentVariableHelper.GetEnvironmentVarOrDefault(
            UrlEnvironmentVariable,
            "http://localhost:5050"
        );
}