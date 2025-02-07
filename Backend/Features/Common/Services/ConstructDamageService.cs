using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQutils.Def;

namespace Mod.DynamicEncounters.Features.Common.Services;

public class ConstructDamageService(IServiceProvider provider) : IConstructDamageService
{
    private readonly IConstructElementsService _constructElementsService =
        provider.GetRequiredService<IConstructElementsService>();

    private readonly IGameplayBank _bank = provider.GetGameplayBank();

    private Dictionary<WeaponTypeScale, IList<AmmoItem>>? AmmoMap { get; set; }

    public Dictionary<WeaponTypeScale, IList<AmmoItem>> GetAllAmmoTypesByWeapon()
    {
        if (AmmoMap != null)
        {
            return AmmoMap;
        }
        
        var dictionary = new Dictionary<WeaponTypeScale, IList<AmmoItem>>();
        
        var ammo = _bank.GetDefinition<Ammo>();

        foreach (var itemId in ammo.GetChildrenIdsRecursive())
        {
            var bo = _bank.GetBaseObject<Ammo>(itemId);
            if (bo == null || bo.Hidden)
            {
                continue;
            }

            var def = _bank.GetDefinition(itemId);
            if (def == null || def.GetChildren().Any())
            {
                continue;
            }

            var key = new WeaponTypeScale(bo.WeaponType, bo.Scale);
            
            dictionary.TryAdd(
                key,
                new List<AmmoItem>()
            );

            dictionary[key].Add(new AmmoItem(
                def.Id,
                def.Name,
                bo
            ));
        }

        AmmoMap = dictionary;

        return AmmoMap;
    }

    public async Task<ConstructDamageData> GetConstructDamage(ulong constructId)
    {
        var weaponUnits = (await _constructElementsService.GetWeaponUnits(constructId)).ToList();

        if (weaponUnits.Count == 0)
        {
            return new ConstructDamageData([]);
        }
        
        var allAmmo = GetAllAmmoTypesByWeapon();
        var items = new List<WeaponItem>();

        foreach (var weaponUnit in weaponUnits)
        {
            var element = await _constructElementsService.GetElement(constructId, weaponUnit.elementId);

            var baseObject = _bank.GetBaseObject<WeaponUnit>(element.elementType);
            var def = _bank.GetDefinition(element);

            if (baseObject == null) continue;

            var ammoKey = new WeaponTypeScale(baseObject.WeaponType, baseObject.Scale);
            if (allAmmo.TryGetValue(ammoKey, out var ammoItems))
            {
                items.Add(new WeaponItem(
                    element.elementId,
                    def.Name,
                    baseObject, 
                    ammoItems
                ));
            }
        }

        return new ConstructDamageData(
            items.DistinctBy(x => x.ItemTypeName)
                .Where(x => x.BaseDamage > 0)
        );
    }
}