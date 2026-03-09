using NpcCommonLib.Data;
using NpcWeaponLib.Data;

namespace NpcWeaponLib.Interfaces;

/// <summary>
/// Reads per-weapon health status from the game server.
/// </summary>
/// <remarks>
/// In the game backend, this queries construct elements via
/// <c>IConstructElementsService.GetDamagingWeaponsEffectiveness()</c>,
/// which reads hitpoint ratios from the Orleans element grains.
/// </remarks>
public interface IWeaponHealthService
{
    /// <summary>
    /// Returns weapon health data keyed by weapon ItemTypeName.
    /// Each value is a list of individual weapon elements of that type.
    /// </summary>
    /// <param name="constructId">Construct whose weapon health to query.</param>
    /// <returns>
    /// Dictionary mapping weapon type names to lists of <see cref="WeaponEffectiveness"/> entries,
    /// one per physical weapon element of that type on the construct.
    /// </returns>
    Task<IDictionary<string, IList<WeaponEffectiveness>>> GetWeaponEffectiveness(ConstructId constructId);
}
