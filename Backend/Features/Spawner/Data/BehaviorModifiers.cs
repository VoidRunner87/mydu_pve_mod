namespace Mod.DynamicEncounters.Features.Spawner.Data;

public class BehaviorModifiers
{
    public WeaponModifiers Weapon { get; set; } = new();
    
    public class WeaponModifiers
    {
        public float Damage { get; set; } = 1;
        public float Accuracy { get; set; } = 1;
        public float CycleTime { get; set; } = 1;
        public float OptimalDistance { get; set; } = 1;
        public float FalloffDistance { get; set; } = 1;
        public float FalloffAimingCone { get; set; } = 1;
        public float FalloffTracking { get; set; } = 1;
        public float OptimalTracking { get; set; } = 1;
        public float OptimalAimingCone { get; set; } = 1;
    }
}