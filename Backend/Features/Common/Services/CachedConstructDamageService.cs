using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Common.Interfaces;

namespace Mod.DynamicEncounters.Features.Common.Services;

public class CachedConstructDamageService(IConstructDamageService service) : IConstructDamageService
{
    private readonly TemporaryMemoryCache<ulong, ConstructDamageData> _constructDamage = new(nameof(_constructDamage), TimeSpan.FromHours(1));

    public Dictionary<WeaponTypeScale, IList<AmmoItem>> GetAllAmmoTypesByWeapon()
    {
        return service.GetAllAmmoTypesByWeapon();
    }

    public Task<ConstructDamageData> GetConstructDamage(ulong constructId)
    {
        return _constructDamage.TryGetOrSetValue(
            constructId,
            () => service.GetConstructDamage(constructId),
            outcome => outcome == null 
        );
    }
}