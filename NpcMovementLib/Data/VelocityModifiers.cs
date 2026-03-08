namespace NpcMovementLib.Data;

public class VelocityModifiers
{
    public const long OneSuInMeters = 200000;

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

    public double OutsideOptimalRange2XAlpha { get; set; } = 2;
    public double OutsideOptimalRangeAlpha { get; set; } = 4;

    public double GetFarDistanceM() => FarDistanceSu * OneSuInMeters;
}

public struct ModifierByDotProduct
{
    public required double Positive { get; set; }
    public required double Negative { get; set; }
}
