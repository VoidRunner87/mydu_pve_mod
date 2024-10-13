using System;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using NQ;

namespace Mod.DynamicEncounters.Features.Common.Services;

public class CachedConstructService(
    IConstructService service,
    TimeSpan constructInfoCacheSpan,
    TimeSpan controlCheckCacheSpan
) : IConstructService
{
    private readonly TemporaryMemoryCache<ulong, ConstructInfoOutcome> _constructInfos = new(nameof(_constructInfos), constructInfoCacheSpan);
    private readonly TemporaryMemoryCache<ulong, Velocities> _velocities = new(nameof(_velocities), constructInfoCacheSpan);
    private readonly TemporaryMemoryCache<ulong, bool> _beingControlled = new(nameof(_beingControlled), controlCheckCacheSpan);
    private readonly TemporaryMemoryCache<ulong, bool> _inSafeZone = new(nameof(_inSafeZone), controlCheckCacheSpan);
    private readonly TemporaryMemoryCache<ulong, bool> _identifyNotification = new(nameof(_identifyNotification), constructInfoCacheSpan);
    private readonly TemporaryMemoryCache<ulong, bool> _attackingNotification = new(nameof(_attackingNotification), constructInfoCacheSpan);

    public async Task<ConstructInfoOutcome> GetConstructInfoAsync(ulong constructId)
    {
        return await _constructInfos.TryGetOrSetValue(
            constructId,
            async () => await service.GetConstructInfoAsync(constructId),
            outcome => !outcome.ConstructExists || outcome.Info == null
        );
    }

    public Task<ConstructTransformOutcome> GetConstructTransformFromDbAsync(ulong constructId)
    {
        return service.GetConstructTransformFromDbAsync(constructId);
    }

    public async Task<ConstructTransformOutcome> GetConstructTransformAsync(ulong constructId)
    {
        var constructInfoOutcome = await GetConstructInfoAsync(constructId);
        
        if (constructInfoOutcome.ConstructExists && constructInfoOutcome.Info == null)
        {
            return await GetConstructTransformFromDbAsync(constructId);
        }

        if (!constructInfoOutcome.ConstructExists)
        {
            return ConstructTransformOutcome.DoesNotExist();
        }

        return new ConstructTransformOutcome(
            constructInfoOutcome.ConstructExists,
            constructInfoOutcome.Info!.rData.position,
            constructInfoOutcome.Info!.rData.rotation
        );
    }

    public Task ResetConstructCombatLock(ulong constructId)
        => service.ResetConstructCombatLock(constructId);

    public Task SetDynamicWreckAsync(ulong constructId, bool isDynamicWreck)
        => service.SetDynamicWreckAsync(constructId, isDynamicWreck);

    public async Task<Velocities> GetConstructVelocities(ulong constructId)
    {
        return await _velocities.TryGetOrSetValue(
            constructId,
            async () =>
            {
                var result = await service.GetConstructVelocities(constructId);
                return new Velocities(result.Linear, result.Angular);
            });
    }

    public Task DeleteAsync(ulong constructId)
        => service.DeleteAsync(constructId);

    public Task SoftDeleteAsync(ulong constructId) 
        => service.SoftDeleteAsync(constructId);

    public Task SetAutoDeleteFromNowAsync(ulong constructId, TimeSpan timeSpan)
        => service.SetAutoDeleteFromNowAsync(constructId, timeSpan);

    public Task<bool> TryVentShieldsAsync(ulong constructId)
        => service.TryVentShieldsAsync(constructId);

    public async Task<bool> IsBeingControlled(ulong constructId)
    {
        return await _beingControlled.TryGetOrSetValue(
            constructId,
            () => service.IsBeingControlled(constructId)
        );
    }

    public IConstructService NoCache()
    {
        return service;
    }

    public Task<bool> Exists(ulong constructId)
    {
        return service.Exists(constructId);
    }

    public Task ActivateShieldsAsync(ulong constructId)
    {
        return service.ActivateShieldsAsync(constructId);
    }

    public async Task<bool> IsInSafeZone(ulong constructId)
    {
        return await _inSafeZone.TryGetOrSetValue(
            constructId,
            () => service.IsInSafeZone(constructId)
        );
    }

    public async Task SendIdentificationNotification(ulong constructId, TargetingConstructData targeting)
    {
        await _identifyNotification.TryGetOrSetValue(
            constructId,
            () => service.SendIdentificationNotification(constructId, targeting)
                .ContinueWith(_ => true)
        );
    }

    public async Task SendAttackingNotification(ulong constructId, TargetingConstructData targeting)
    {
        await _attackingNotification.TryGetOrSetValue(
            constructId,
            () => service.SendAttackingNotification(constructId, targeting)
                .ContinueWith(_ => true)
        );
    }
}