using System;
using System.Linq;
using System.Threading.Tasks;
using Backend;
using Backend.Database;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;
using NQ.Visibility;
using NQutils.Def;
using NQutils.Sql;
using Orleans;
using Quat = NQ.Quat;
using Vec3 = NQ.Vec3;

namespace Mod.DynamicEncounters.Features.Common.Services;

public class ConstructService(IServiceProvider provider) : IConstructService
{
    private readonly ILogger<ConstructService> _logger = provider.CreateLogger<ConstructService>();
    private readonly IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>();
    private readonly IClusterClient _orleans = provider.GetOrleans();

    public async Task<ConstructInfoOutcome> GetConstructInfoAsync(ulong constructId)
    {
        try
        {
            if (constructId == 0)
            {
                return ConstructInfoOutcome.DoesNotExist();
            }

            var info = await _orleans.GetConstructInfoGrain(constructId).Get();

            return new ConstructInfoOutcome(true, info);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to fetch construct information for {ConstructId}", constructId);

            if (await ExistsAndNotDeleted(constructId))
            {
                // indicates that the construct exists, but we failed to get info on it
                return new ConstructInfoOutcome(true, null);
            }

            return ConstructInfoOutcome.DoesNotExist();
        }
    }

    public async Task<ConstructTransformOutcome> GetConstructTransformAsync(ulong constructId)
    {
        var constructInfoOutcome = await GetConstructInfoAsync(constructId);
        if (!constructInfoOutcome.ConstructExists)
        {
            return ConstructTransformOutcome.DoesNotExist();
        }

        if (constructInfoOutcome.ConstructExists && constructInfoOutcome.Info == null)
        {
            return await GetConstructTransformFromDbAsync(constructId);
        }

        var info = constructInfoOutcome.Info;

        return new ConstructTransformOutcome(
            info != null,
            info?.rData.position ?? new Vec3(),
            info?.rData.rotation ?? Quat.Identity
        );
    }

    public async Task<ConstructTransformOutcome> GetConstructTransformFromDbAsync(ulong constructId)
    {
        using var db = _factory.Create();
        db.Open();

        var result = (await db.QueryAsync<ConstructTransformRow>(
            """
            SELECT position_x, position_y, position_z, rotation_x, rotation_y, rotation_z, rotation_w 
            FROM public.construct WHERE id = @constructId
            """,
            new { constructId = (long)constructId }
        )).ToList();

        if (result.Count == 0)
        {
            return ConstructTransformOutcome.DoesNotExist();
        }

        var item = result[0];

        return new ConstructTransformOutcome(
            true,
            new Vec3
            {
                x = item.position_x,
                y = item.position_y,
                z = item.position_z
            },
            new Quat
            {
                x = item.rotation_x,
                y = item.rotation_y,
                z = item.rotation_z,
                w = item.rotation_w,
            }
        );
    }

    public struct ConstructTransformRow
    {
        public double position_x;
        public double position_y;
        public double position_z;
        public float rotation_x;
        public float rotation_y;
        public float rotation_z;
        public float rotation_w;
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

    public async Task SoftDeleteAsync(ulong constructId)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync(
            $"""
             UPDATE public.construct SET deleted_at = NOW() WHERE id = @constructId AND deleted_at IS NULL
             """
            , new
            {
                constructId = (long)constructId
            }
        );

        var internalClient = provider.GetRequiredService<Internal.InternalClient>();
        await internalClient.RemoveConstructAsync(constructId);
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

    public IConstructService NoCache()
    {
        return this;
    }

    public async Task<bool> Exists(ulong constructId)
    {
        using var db = _factory.Create();
        db.Open();

        var count = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(0) FROM public.construct WHERE id = @constructId",
            new { constructId = (long)constructId }
        );

        return count > 0;
    }

    public async Task<bool> ExistsAndNotDeleted(ulong constructId)
    {
        using var db = _factory.Create();
        db.Open();

        var count = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(0) FROM public.construct WHERE id = @constructId AND deleted_at IS NULL",
            new { constructId = (long)constructId }
        );

        return count > 0;
    }

    public async Task ActivateShieldsAsync(ulong constructId)
    {
        var constructElementsGrain = _orleans.GetConstructElementsGrain(constructId);
        var shields = await constructElementsGrain.GetElementsOfType<ShieldGeneratorUnit>();

        if (shields.Count == 0)
        {
            return;
        }

        try
        {
            var sql = provider.GetRequiredService<ISql>();
            await sql.SetShieldEnabled(constructId, true);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to Enable Shields");
        }
    }

    public Task<bool> IsInSafeZone(ulong constructId)
    {
        return _orleans.GetConstructGrain(constructId).IsInSafeZone();
    }

    public Task SendIdentificationNotification(ulong constructId, TargetingConstructData targeting)
    {
        return _orleans.GetConstructGrain(constructId)
            .ConstructStartIdentifying(
                targeting,
                0UL
            );
    }

    public Task SendAttackingNotification(ulong constructId, TargetingConstructData targeting)
    {
        return _orleans.GetConstructGrain(constructId)
            .ConstructStartAttacking(
                targeting,
                0UL
            );
    }
}