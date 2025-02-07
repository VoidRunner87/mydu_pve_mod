using NQ;
using NQ.Interfaces;

namespace Mod.DynamicEncounters.Overrides.Actions.Data;

public class ShootWeaponData
{
    public string ShooterName { get; set; }
    public Vec3 ShooterPosition { get; set; }
    public ulong ShooterPlayerId { get; set; }
    public ulong ShooterConstructId { get; set; }
    public ulong ShooterConstructSize { get; set; }
    public ulong TargetConstructId { get; set; }
    public Vec3 LocalHitPosition { get; set; }
    public SentinelWeapon Weapon { get; set; }
    public double CrossSection { get; set; }
    public bool DamagesVoxel { get; set; }
}