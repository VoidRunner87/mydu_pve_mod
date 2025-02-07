using Mod.DynamicEncounters.Helpers;
using NQ;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Services;

public class PIDController
{
    private readonly double _kp; // Proportional gain
    private readonly double _ki; // Integral gain
    private readonly double _kd; // Derivative gain

    private Vec3 _integral; // Accumulated error
    private Vec3 _previousError; // Previous error for derivative calculation

    public PIDController(double kp, double ki, double kd)
    {
        _kp = kp;
        _ki = ki;
        _kd = kd;

        _integral = new Vec3 { x = 0, y = 0, z = 0 };
        _previousError = new Vec3 { x = 0, y = 0, z = 0 };
    }

    public Vec3 Compute(Vec3 currentPosition, Vec3 targetPosition, double deltaTime, double deadZone)
    {
        // Calculate error (distance vector to target)
        var error = targetPosition - currentPosition;
        
        if (error.Size() < deadZone)
        {
            return new Vec3 { x = 0, y = 0, z = 0 };
        }

        // Proportional term
        var proportional = error * _kp;

        // Integral term (accumulating error over time)
        _integral = _integral + (error * deltaTime);
        var integralTerm = _integral * _ki;

        // Derivative term (rate of change of error)
        var derivative = (error - _previousError) / deltaTime * _kd;

        // Update previous error
        _previousError = error;

        // Compute PID output (desired acceleration)
        return proportional + integralTerm + derivative;
    }
}
