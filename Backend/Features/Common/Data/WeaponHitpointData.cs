namespace Mod.DynamicEncounters.Features.Common.Data;

public class WeaponEffectivenessData
{
    public required string Name { get; set; }
    public required double HitPointsRatio { get; set; }

    public bool IsDestroyed() => HitPointsRatio <= 0.01f;
}