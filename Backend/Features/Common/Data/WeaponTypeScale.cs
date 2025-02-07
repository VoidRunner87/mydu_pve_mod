using NQutils.Def;

namespace Mod.DynamicEncounters.Features.Common.Data;

public struct WeaponTypeScale(WeaponType weaponType, string scale)
{
    public WeaponType WeaponType { get; } = weaponType;
    public string Scale { get; } = scale;
}