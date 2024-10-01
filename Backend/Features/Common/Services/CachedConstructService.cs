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
    private readonly TemporaryMemoryCache<ulong, ConstructInfo> _constructInfos = new(constructInfoCacheSpan);
    private readonly TemporaryMemoryCache<ulong, Velocities> _velocities = new(constructInfoCacheSpan);
    private readonly TemporaryMemoryCache<ulong, bool> _beingControlled = new(controlCheckCacheSpan);
    private readonly TemporaryMemoryCache<ulong, bool> _inSafeZone = new(controlCheckCacheSpan);
    private readonly TemporaryMemoryCache<ulong, bool> _identifyNotification = new(constructInfoCacheSpan);
    private readonly TemporaryMemoryCache<ulong, bool> _attackingNotification = new(constructInfoCacheSpan);

    public async Task<ConstructInfo?> GetConstructInfoAsync(ulong constructId)
    {
        return await _constructInfos.TryGetValue(
            constructId,
            async () => await service.GetConstructInfoAsync(constructId)
        );
    }

    public Task ResetConstructCombatLock(ulong constructId)
        => service.ResetConstructCombatLock(constructId);

    public Task SetDynamicWreckAsync(ulong constructId, bool isDynamicWreck)
        => service.SetDynamicWreckAsync(constructId, isDynamicWreck);

    public async Task<Velocities> GetConstructVelocities(ulong constructId)
    {
        return await _velocities.TryGetValue(
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
        return await _beingControlled.TryGetValue(
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
        return await _inSafeZone.TryGetValue(
            constructId,
            () => service.IsInSafeZone(constructId)
        );
    }

    public async Task SendIdentificationNotification(ulong constructId, TargetingConstructData targeting)
    {
        await _identifyNotification.TryGetValue(
            constructId,
            () => service.SendIdentificationNotification(constructId, targeting)
                .ContinueWith(_ => true)
        );
    }

    public async Task SendAttackingNotification(ulong constructId, TargetingConstructData targeting)
    {
        await _attackingNotification.TryGetValue(
            constructId,
            () => service.SendAttackingNotification(constructId, targeting)
                .ContinueWith(_ => true)
        );
    }
}