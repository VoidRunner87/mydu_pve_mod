using System.Numerics;

namespace NpcMovementLib.Math;

public static class VelocityHelper
{
    public static double CalculateBrakingDistance(double velocity, double deceleration)
    {
        return System.Math.Pow(velocity, 2) / (2 * deceleration);
    }

    public static double CalculateBrakingTime(double initialVelocity, double deceleration)
    {
        if (deceleration <= 0) return 60 * 60;
        if (initialVelocity <= 0) return 0;
        return initialVelocity / deceleration;
    }

    public static bool ShouldStartBraking(Vector3 currentPosition, Vector3 targetPosition,
        Vector3 currentVelocity, double deceleration)
    {
        double remainingDistance = Vector3.Distance(currentPosition, targetPosition);
        var brakingDistance = CalculateBrakingDistance(currentVelocity.Length(), deceleration);
        return remainingDistance <= brakingDistance;
    }

    public static double CalculateTimeToReachVelocity(
        double initialVelocity, double targetVelocity, double acceleration)
    {
        if (acceleration == 0) return 60 * 60;
        var time = (targetVelocity - initialVelocity) / acceleration;
        return System.Math.Abs(time);
    }

    public static Vec3 LinearInterpolateWithAcceleration(
        Vec3 start, Vec3 end, ref Vec3 velocity, Vec3 acceleration,
        double clampSize, double deltaTime, bool handleOvershoot = false)
    {
        var direction = end - start;
        var distance = direction.Size();

        if (distance < 0.001) return end;

        var accelFactor = 0.5d;

        var displacement = new Vec3(
            velocity.X * deltaTime + accelFactor * acceleration.X * deltaTime * deltaTime,
            velocity.Y * deltaTime + accelFactor * acceleration.Y * deltaTime * deltaTime,
            velocity.Z * deltaTime + accelFactor * acceleration.Z * deltaTime * deltaTime
        );

        velocity = new Vec3(
            velocity.X + acceleration.X * deltaTime,
            velocity.Y + acceleration.Y * deltaTime,
            velocity.Z + acceleration.Z * deltaTime
        );

        velocity = velocity.ClampToSize(clampSize);

        var newPosition = start + displacement;

        if (double.IsNaN(newPosition.X) || double.IsNaN(newPosition.Y) || double.IsNaN(newPosition.Z) ||
            double.IsNaN(velocity.X) || double.IsNaN(velocity.Y) || double.IsNaN(velocity.Z))
        {
            newPosition = end;
        }

        if (handleOvershoot)
        {
            if ((newPosition - start).Size() > distance)
            {
                newPosition = end;
                velocity = Vec3.Zero;
            }
        }

        return newPosition;
    }

    public static Vec3 LinearInterpolateWithAccelerationV2(
        Vec3 start, Vec3 end, ref Vec3 velocity, Vec3 acceleration,
        double clampSize, double velocitySizeGoal, double deltaTime,
        bool handleOvershoot = false)
    {
        var direction = end - start;
        var distance = direction.Size();

        if (distance < 0.001) return end;

        direction = direction.Normalized();

        double currentVelocitySize = velocity.Size();
        double newVelocitySize;

        if (currentVelocitySize > velocitySizeGoal)
        {
            double decelerationMagnitude = acceleration.Size();
            newVelocitySize = System.Math.Max(currentVelocitySize - decelerationMagnitude * deltaTime, velocitySizeGoal);
        }
        else
        {
            newVelocitySize = System.Math.Min(currentVelocitySize + acceleration.Size() * deltaTime, velocitySizeGoal);
        }

        velocity = direction * newVelocitySize;

        var accelFactor = 0.5d;
        var displacement = new Vec3(
            velocity.X * deltaTime + accelFactor * acceleration.X * deltaTime * deltaTime,
            velocity.Y * deltaTime + accelFactor * acceleration.Y * deltaTime * deltaTime,
            velocity.Z * deltaTime + accelFactor * acceleration.Z * deltaTime * deltaTime
        );

        var newPosition = start + displacement;

        velocity = velocity.ClampToSize(clampSize);

        if (double.IsNaN(newPosition.X) || double.IsNaN(newPosition.Y) || double.IsNaN(newPosition.Z) ||
            double.IsNaN(velocity.X) || double.IsNaN(velocity.Y) || double.IsNaN(velocity.Z))
        {
            newPosition = end;
            velocity = Vec3.Zero;
        }

        if (handleOvershoot)
        {
            if ((newPosition - start).Size() > distance)
            {
                newPosition = end;
                velocity = Vec3.Zero;
            }
        }

        return newPosition;
    }

    public static Vec3 LinearInterpolateWithVelocity(
        Vec3 start, Vec3 end, ref Vec3 velocity, Vec3 acceleration,
        double clampSize, double deltaTime)
    {
        var direction = end - start;
        var distance = direction.Size();

        if (distance < 0.001) return end;

        velocity = new Vec3(
            velocity.X + acceleration.X * deltaTime,
            velocity.Y + acceleration.Y * deltaTime,
            velocity.Z + acceleration.Z * deltaTime
        );

        velocity = velocity.ClampToSize(clampSize);

        var newPosition = new Vec3(
            start.X + velocity.X * deltaTime,
            start.Y + velocity.Y * deltaTime,
            start.Z + velocity.Z * deltaTime
        );

        var newDirection = end - newPosition;
        var newDistance = newDirection.Size();

        if (newDistance < 0.001 || newDistance < acceleration.Size() * deltaTime)
        {
            newPosition = end;
        }

        if (double.IsNaN(newPosition.X) || double.IsNaN(newPosition.Y) || double.IsNaN(newPosition.Z) ||
            double.IsNaN(velocity.X) || double.IsNaN(velocity.Y) || double.IsNaN(velocity.Z))
        {
            newPosition = end;
        }

        return newPosition;
    }

    public static Vec3 ApplyBraking(Vec3 start, ref Vec3 velocity, double decelerationRate, double deltaTime)
    {
        if (velocity.Size() < 0.001)
        {
            velocity = Vec3.Zero;
            return start;
        }

        var decelerationMagnitude = decelerationRate * deltaTime;

        if (System.Math.Abs(velocity.X) <= decelerationMagnitude)
            velocity.X = 0;
        else
            velocity.X += velocity.X > 0 ? -decelerationMagnitude : decelerationMagnitude;

        if (System.Math.Abs(velocity.Y) <= decelerationMagnitude)
            velocity.Y = 0;
        else
            velocity.Y += velocity.Y > 0 ? -decelerationMagnitude : decelerationMagnitude;

        if (System.Math.Abs(velocity.Z) <= decelerationMagnitude)
            velocity.Z = 0;
        else
            velocity.Z += velocity.Z > 0 ? -decelerationMagnitude : decelerationMagnitude;

        var displacement = new Vec3(
            velocity.X * deltaTime,
            velocity.Y * deltaTime,
            velocity.Z * deltaTime
        );

        return start + displacement;
    }

    public static Vec3 CalculateFuturePosition(Vec3 currentPosition, Vec3 velocity, Vec3 acceleration,
        double deltaTime)
    {
        return new Vec3(
            currentPosition.X + velocity.X * deltaTime + 0.5 * acceleration.X * deltaTime * deltaTime,
            currentPosition.Y + velocity.Y * deltaTime + 0.5 * acceleration.Y * deltaTime * deltaTime,
            currentPosition.Z + velocity.Z * deltaTime + 0.5 * acceleration.Z * deltaTime * deltaTime
        );
    }

    public static double CalculateTimeToReachDistance(
        Vec3 position1, Vec3 velocity1, Vec3 position2, Vec3 velocity2, double targetDistance)
    {
        var relativePosition = position2 - position1;
        var relativeVelocity = velocity2 - velocity1;

        var currentDistance = relativePosition.Size();
        var relativeSpeed = relativePosition.Dot(relativeVelocity) / currentDistance;

        if (System.Math.Abs(relativeSpeed) < 1e-6)
        {
            return System.Math.Abs(currentDistance - targetDistance) < 0.01 ? 0 : double.PositiveInfinity;
        }

        var time = (currentDistance - targetDistance) / relativeSpeed;
        return time >= 0 ? time : double.PositiveInfinity;
    }

    public static Vec3 CalculateAcceleration(Vec3 initialPosition, Vec3 finalPosition,
        Vec3 initialVelocity, double deltaTime)
    {
        if (deltaTime <= 0) return Vec3.Zero;

        var displacement = finalPosition - initialPosition;

        return new Vec3(
            2 * (displacement.X - initialVelocity.X * deltaTime) / (deltaTime * deltaTime),
            2 * (displacement.Y - initialVelocity.Y * deltaTime) / (deltaTime * deltaTime),
            2 * (displacement.Z - initialVelocity.Z * deltaTime) / (deltaTime * deltaTime)
        );
    }
}
