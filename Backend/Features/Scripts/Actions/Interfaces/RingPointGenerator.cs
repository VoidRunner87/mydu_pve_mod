using System;
using System.Numerics;
using NQ;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

public class RingPointGenerator : IPointGenerator
{
    private readonly double _minRadius;
    private readonly double _maxRadius;
    private readonly double _height;
    private readonly Quaternion _rotation;

    public RingPointGenerator(double minRadius, double maxRadius, double height, Quaternion rotation)
    {
        if (minRadius < 0) throw new ArgumentException("Min radius cannot be negative.", nameof(minRadius));
        if (maxRadius <= 0) throw new ArgumentException("Max radius must be greater than zero.", nameof(maxRadius));
        if (minRadius >= maxRadius) throw new ArgumentException("Min radius must be less than max radius.", nameof(minRadius));
        if (height <= 0) throw new ArgumentException("Height must be greater than zero.", nameof(height));

        _minRadius = minRadius;
        _maxRadius = maxRadius;
        _height = height;
        _rotation = rotation;
    }

    public Vec3 NextPoint(Random random)
    {
        // 1. Generate a random angle (0 to 2*PI) and radius in the annulus region (minRadius to maxRadius)
        var angle = 2 * Math.PI * random.NextDouble();

        // Uniform distribution within the ring: scale radius appropriately
        var radius = Math.Sqrt(random.NextDouble() * (Math.Pow(_maxRadius, 2) - Math.Pow(_minRadius, 2)) + Math.Pow(_minRadius, 2));

        // 2. Calculate X and Z in local space
        var x = radius * Math.Cos(angle);
        var z = radius * Math.Sin(angle);

        // 3. Generate a random Y (height) within the cylinder
        var y = (random.NextDouble() - 0.5) * _height; // Centered on 0

        // 4. Create the point in local space
        var localPoint = new Vector3((float)x, (float)y, (float)z);

        // 5. Rotate the point using the given quaternion
        var rotatedPoint = Vector3.Transform(localPoint, _rotation);

        // 6. Return the resulting point as a Vec3
        return new Vec3 { x = rotatedPoint.X, y = rotatedPoint.Y, z = rotatedPoint.Z };
    }
}