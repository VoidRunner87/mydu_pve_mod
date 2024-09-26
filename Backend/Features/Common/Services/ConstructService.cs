using System;
using System.Threading.Tasks;
using Backend;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;
using NQutils.Def;
using Orleans;

namespace Mod.DynamicEncounters.Features.Common.Services;

public class ConstructService(IServiceProvider provider) : IConstructService
{
    private readonly ILogger<ConstructService> _logger = provider.CreateLogger<ConstructService>();
    private readonly IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>();
    private readonly IClusterClient _orleans = provider.GetOrleans();

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
            new { id = (long)constructId }
        );
    }

    public async Task<Velocities> GetConstructVelocities(ulong constructId)
    {
        var result = await _orleans.GetConstructGrain(constructId)
            .GetConstructVelocity();

        return new Velocities(result.velocity, result.angVelocity);
    }

    public Task DeleteAsync(ulong constructId)
    {
        return _orleans.GetConstructGCGrain().DeleteConstruct(constructId);
    }

    public async Task SetAutoDeleteFromNowAsync(ulong constructId, TimeSpan timeSpan)
    {
        var bank = provider.GetGameplayBank();
        var gcConfig = bank.GetBaseObject<ConstructGCConfig>();
        var deleteHours = gcConfig.abandonedConstructDeleteDelayHours;
        
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync(
            $"""
            UPDATE public.construct SET abandoned_at = NOW() - INTERVAL '{deleteHours} HOURS' + INTERVAL '{timeSpan.ToPostgresInterval()}'
            WHERE id = @constructId
            """
            , new
            {
                constructId = (long)constructId
            }
        );
    }

    public async Task<bool> TryVentShieldsAsync(ulong constructId)
    {
        try
        {
            var constructInfoGrain = _orleans.GetConstructInfoGrain(constructId);
            var constructInfo = await constructInfoGrain.Get();

            if (!constructInfo.mutableData.shieldState.hasShield)
            {
                return false;
            }
            
            var shieldPercent = constructInfo.mutableData.shieldState.shieldHpRatio * 100;
            
            _logger.LogInformation("Construct {Construct} Shield Percent {Percent}", constructId, shieldPercent);
            
            if (constructInfo.mutableData.shieldState.isVenting)
            {
                _logger.LogInformation("Construct {Construct} Already Venting. Shield at {Percent}", 
                    constructId,
                    shieldPercent
                );
                return true;
            }
            
            var constructFightGrain = _orleans.GetConstructFightGrain((ulong)constructId);
            await constructFightGrain.StartVenting(4);

            return true;
        }
        catch (Exception)
        {
            _logger.LogWarning("Could not vent shields. On Cooldown or Destroyed");
            
            return false;
        }
    }

    public Task<bool> IsBeingControlled(ulong constructId)
    {
        return _orleans.GetConstructGrain(constructId).IsBeingControlled();
    }

    public void Invalidate()
    {
    }
}