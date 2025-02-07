using NQutils.Def;

namespace Mod.DynamicEncounters.Features.Common.Data;

public class AmmoItem(ulong itemTypeId, string itemTypeName, Ammo ammo)
{
    public ulong ItemTypeId { get; set; } = itemTypeId;
    public string ItemTypeName { get; set; } = itemTypeName;
    public string Scale { get; set; } = ammo.Scale;
    public int Level { get; set; } = ammo.Level;
    public DamageType DamageType { get; set; } = ammo.DamageType;
    public double UnitVolume { get; set; } = ammo.UnitVolume;
}