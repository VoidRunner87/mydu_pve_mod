namespace NpcMovementLib.Data;

public class MovementConfig
{
    public double AccelerationG { get; set; } = 15;
    public float RotationSpeed { get; set; } = 0.5f;
    public double MinSpeedKph { get; set; } = 2000;
    public double MaxSpeedKph { get; set; } = 20000;
    public double RealismFactor { get; set; }
    public double TargetDistance { get; set; } = 20000;

    public double MinVelocity => MinSpeedKph / 3.6d;
    public double MaxVelocity => MaxSpeedKph / 3.6d;
}
