using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Common.Data;

namespace Mod.DynamicEncounters.Features.Common.Interfaces;

public interface IConstructDamageService
{
    Dictionary<WeaponTypeScale, IList<AmmoItem>> GetAllAmmoTypesByWeapon();
    Task<ConstructDamageData> GetConstructDamage(ulong constructId);
}