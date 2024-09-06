using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;

namespace Mod.DynamicEncounters.Features.Common.Services;

public class ConstructService(IServiceProvider provider) : IConstructService
{
    private readonly ILogger<ConstructService> _logger = provider.CreateLogger<ConstructService>();
    private readonly IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>();

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

    public async Task SetDynamicWreckAsync(ulong constructId, bool isDynamicWreck)
    {
        if (constructId == 0)
        {
            return;
        }

        // TODO move this to a repo
        using var db = _factory.Create();
        db.Open();

        var value = isDynamicWreck ? "true" : "false";
        const string dynamicWreckProp = "{serverProperties,isDynamicWreck}";
        
        await db.ExecuteAsync(
            $"""
            UPDATE public.construct
            SET json_properties = jsonb_set(json_properties, '{dynamicWreckProp}', '{value}')
            WHERE id = @id;
            """,
            new {id = (long)constructId}
        );
    }
}