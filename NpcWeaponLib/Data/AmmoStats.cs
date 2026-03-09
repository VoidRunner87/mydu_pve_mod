namespace NpcWeaponLib.Data;

/// <summary>
/// Ammunition properties for a weapon system. Pure data — no game SDK dependencies.
/// </summary>
/// <remarks>
/// Ported from <c>Mod.DynamicEncounters.Features.Common.Data.AmmoItem</c>.
/// The original class wraps <c>NQutils.Def.Ammo</c>; this version is standalone.
/// </remarks>
public class AmmoStats
{
    /// <summary>Unique item type identifier from the game bank.</summary>
    public required ulong ItemTypeId { get; set; }

    /// <summary>Internal type name (e.g., "AmmoCannonSmallKineticAdvancedAgile").</summary>
    public required string ItemTypeName { get; set; }

    /// <summary>Weapon scale category (e.g., "xs", "s", "m", "l").</summary>
    public required string Scale { get; set; }

    /// <summary>Ammo tier level (1-5). Used with <see cref="FiringInput.AmmoTier"/> to filter compatible ammo.</summary>
    public required int Level { get; set; }

    /// <summary>
    /// Damage type identifier (antimatter, electromagnetic, kinetic, thermic).
    /// Stored as string to avoid dependency on <c>NQutils.Def.DamageType</c> enum.
    /// </summary>
    public required string DamageType { get; set; }

    /// <summary>
    /// Volume of a single ammo unit in litres.
    /// Used to calculate magazine capacity: <c>Floor(MagazineVolume * magBuff / UnitVolume)</c>.
    /// </summary>
    public required double UnitVolume { get; set; }
}
