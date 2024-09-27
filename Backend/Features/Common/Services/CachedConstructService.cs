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
}