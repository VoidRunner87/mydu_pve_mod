using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;
using Orleans;

namespace Mod.DynamicEncounters.Features.Common.Services;

public class ConstructService(IServiceProvider provider) : IConstructService
{
    private readonly ILogger<ConstructService> _logger = provider.CreateLogger<ConstructService>();
    private readonly IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>();
    private IClusterClient _orleans = provider.GetOrleans();

    public async Task<ConstructInfo?> GetConstructInfoAsync(ulong constructId)
    {
        try
        {
            if (constructId == 0)
            {
                return null;
            }

            return await _orleans.GetConstructInfoGrain(constructId).Get();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to fetch construct information for {ConstructId}", constructId);

            return null;
        }
    }

    public async Task ResetConstructCombatLock(ulong constructId)
    {
        var constructInfoGrain = provider.GetOrleans().GetConstructInfoGrain(constructId);
        await constructInfoGrain.Update(new ConstructInfoUpdate
        {
            pvpTimerExpiration = TimePoint.Now(),
            constructId = constructId,
            // shieldState = new ShieldState
            // {
            //     isActive = false
            // }
        });
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

    public async Task<Velocities> GetConstructVelocities(ulong constructId)
    {
        var result = await _orleans.GetConstructGrain(constructId)
            .GetConstructVelocity();

        return new Velocities(result.velocity, result.angVelocity);
    }
}