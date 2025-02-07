using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Overrides.Common.Data;
using Mod.DynamicEncounters.Overrides.Common.Interfaces;
using NQ;
using NQ.Interfaces;
using NQutils.Def;
using Orleans;

namespace Mod.DynamicEncounters.Overrides.Common.Services;

public class ConstructService(IServiceProvider provider) : IConstructService
{
    private readonly ILogger<ConstructService> _logger = provider.GetRequiredService<ILoggerFactory>()
        .CreateLogger<ConstructService>();

    private readonly IClusterClient _orleans = provider.GetRequiredService<IClusterClient>();

    private readonly TemporaryMemoryCache<ulong, ConstructInfo> _constructInfo =
        new(
            nameof(_constructInfo),
            TimeSpan.FromSeconds(5)
        );

    private readonly TemporaryMemoryCache<ulong, double> _coreStressRatio =
        new(
            nameof(_coreStressRatio),
            TimeSpan.FromSeconds(5)
        );

    public async Task<ConstructItem?> GetConstructInfoCached(ulong constructId)
    {
        try
        {
            var constructInfo = await _constructInfo.TryGetOrSetValue(
                constructId,
                async () => await GetConstructInfo(constructId),
                info => info == null
            );

            return new ConstructItem
            {
                Id = constructInfo.rData.constructId,
                Name = constructInfo.rData.name,
                Size = constructInfo.rData.geometry.size,
                Kind = constructInfo.kind,
                ShieldRatio = constructInfo.mutableData.shieldState.shieldHpRatio
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to Get Construct Info");

            return null;
        }
    }

    public Task<double> GetCoreStressRatioCached(ulong constructId)
    {
        return _coreStressRatio.TryGetOrSetValue(
            constructId,
            () => GetCoreStressRatio(constructId),
            _ => false
        );
    }

    public async Task<ElementId> GetCoreUnit(ulong constructId)
    {
        return (await _orleans.GetConstructElementsGrain(constructId).GetElementsOfType<CoreUnit>()).SingleOrDefault();
    }

    public async Task<double> GetCoreStressRatio(ulong constructId)
    {
        var coreUnitId = await GetCoreUnit(constructId);

        var coreElement = await _orleans.GetConstructElementsGrain(constructId).GetElement(coreUnitId);

        return GetCoreStressPercentage(coreElement);
    }

    private static float GetCoreStressPercentage(ElementInfo elementInfo)
    {
        if (TryGetDoubleValue(elementInfo.properties, "stressMaxHp", out var stressMaxHp))
        {
            if (TryGetDoubleValue(elementInfo.properties, "stressCurrentHp", out var stressCurrentHp))
            {
                // Ship without Voxels - then no CCS
                if (stressMaxHp == 0)
                {
                    return 0;
                }

                if (stressMaxHp <= 0)
                {
                    return 1;
                }

                return (float)((float)stressCurrentHp / stressMaxHp);
            }
        }

        return 1;
    }

    private static bool TryGetDoubleValue(Dictionary<string, PropertyValue> propertyValues, string name,
        out double value)
    {
        if (propertyValues.TryGetValue(name, out var propertyValue))
        {
            value = propertyValue.doubleValue;
            return true;
        }

        value = default;
        return false;
    }

    private async Task<ConstructInfo?> GetConstructInfo(ulong constructId)
    {
        try
        {
            var constructInfoGrain = _orleans.GetConstructInfoGrain(constructId);
            return await constructInfoGrain.Get();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to Fetch non cached construct info. ConstructId {ConstructId}", constructId);

            return null;
        }
    }
}