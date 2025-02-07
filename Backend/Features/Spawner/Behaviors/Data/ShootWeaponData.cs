using NQ;
using NQ.Interfaces;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Data;

public class ShootWeaponData
{
    public required string ShooterName { get; set; }
    public required Vec3 ShooterPosition { get; set; }
    public required ulong ShooterPlayerId { get; set; }
    public required ulong ShooterConstructId { get; set; }
    public required ulong ShooterConstructSize { get; set; }
    public required ulong TargetConstructId { get; set; }
    public required Vec3 LocalHitPosition { get; set; }
    public required SentinelWeapon Weapon { get; set; }
    public required double CrossSection { get; set; }
    public required bool DamagesVoxel { get; set; }
}