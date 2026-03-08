using NpcMovementLib.Math;

namespace NpcMovementLib.Strategies;

public class PIDController
{
    private readonly double _kp;
    private readonly double _ki;
    private readonly double _kd;

    private Vec3 _integral;
    private Vec3 _previousError;

    public PIDController(double kp, double ki, double kd)
    {
        _kp = kp;
        _ki = ki;
        _kd = kd;

        _integral = Vec3.Zero;
        _previousError = Vec3.Zero;
    }

    public Vec3 Compute(Vec3 currentPosition, Vec3 targetPosition, double deltaTime, double deadZone)
    {
        var error = targetPosition - currentPosition;

        if (error.Size() < deadZone)
        {
            return Vec3.Zero;
        }

        var proportional = error * _kp;

        _integral = _integral + error * deltaTime;
        var integralTerm = _integral * _ki;

        var derivative = (error - _previousError) / deltaTime * _kd;

        _previousError = error;

        return proportional + integralTerm + derivative;
    }
}
