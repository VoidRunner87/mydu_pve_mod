using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;

namespace Mod.DynamicEncounters.Features.Common.Services;

public class ConstructService(IServiceProvider provider) : IConstructService
{
    private readonly ILogger<ConstructService> _logger = provider.CreateLogger<ConstructService>();
    
    public async Task<ConstructInfo?> GetConstructInfoAsync(ulong constructId)
    {
        try
        {
            if (constructId == 0)
            {
                return null;
            }
            
            return await provider.GetOrleans().GetConstructInfoGrain(constructId).Get();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to fetch construct information for {ConstructId}", constructId);

            return null;
        }
    }
}