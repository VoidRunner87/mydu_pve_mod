using Mod.DynamicEncounters.Helpers;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Features.Spawner.Data;

public class BehaviorModifiers
{
    public WeaponModifiers Weapon { get; set; } = new();
    public VelocityModifiers Velocity { get; set; } = new();

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

    public class VelocityModifiers
    {
        public bool Enabled { get; set; } = true;
        public bool BoosterEnabled { get; set; } = false;
        public double BoosterAccelerationG { get; set; } = 5d;
        public double FarDistanceSu { get; set; } = 1.5d;
        public double TooCloseDistanceM { get; set; } = 15000;
        public double BrakeDistanceFactor { get; set; } = 2d;

        public ModifierByDotProduct OutsideOptimalRange2X { get; set; }
            = new() { Negative = 0.5d, Positive = 1.5d };

        public ModifierByDotProduct OutsideOptimalRange { get; set; }
            = new() { Negative = 0.25d, Positive = 1.2d };

        public ModifierByDotProduct InsideOptimalRange { get; set; }
            = new() { Negative = 1d, Positive = 1d };

        public double GetFarDistanceM() => FarDistanceSu * DistanceHelpers.OneSuInMeters;
        
        [JsonProperty] public double OutsideOptimalRange2XAlpha { get; set; } = 2;
        [JsonProperty] public double OutsideOptimalRangeAlpha { get; set; } = 4;
    }

    public struct ModifierByDotProduct
    {
        public required double Positive { get; set; }
        public required double Negative { get; set; }
    }
}