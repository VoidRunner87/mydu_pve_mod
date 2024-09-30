using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Helpers;
using NQ;

namespace Mod.DynamicEncounters.Common.Vector;

public static class VelocityHelper
{
    public static Vec3 LinearInterpolateWithVelocity(
        Vec3 start, 
        Vec3 end, 
        ref Vec3 velocity, 
        Vec3 acceleration,
        double clampSize,
        double deltaTime)
    {
        var logger = ModBase.ServiceProvider.CreateLogger<ModBase>();
        
        // Calculate direction and distance to the end
        var direction = new Vec3
        {
            x = end.x - start.x,
            y = end.y - start.y,
            z = end.z - start.z
        };

        var distance = direction.Size();

        // Check if distance is very small (to avoid division by zero)
        if (distance < 0.001)
        {
            return end;
        }

        // Update velocity based on acceleration
        velocity = new Vec3
        {
            x = velocity.x + acceleration.x * deltaTime,
            y = velocity.y + acceleration.y * deltaTime,
            z = velocity.z + acceleration.z * deltaTime
        };

        velocity = velocity.ClampToSize(clampSize);

        // Calculate the new position based on the updated velocity
        var newPosition = new Vec3
        {
            x = start.x + velocity.x * deltaTime,
            y = start.y + velocity.y * deltaTime,
            z = start.z + velocity.z * deltaTime
        };

        // Calculate the new distance to the end
        var newDirection = new Vec3
        {
            x = end.x - newPosition.x,
            y = end.y - newPosition.y,
            z = end.z - newPosition.z
        };

        var newDistance = newDirection.Size();

        // Check if the object is close to the target and the distance change is smaller than acceleration
        if (newDistance < 0.001 || newDistance < acceleration.Size() * deltaTime)
        {
            // Close to the destination; set position to the end and stop velocity
            newPosition = new Vec3 { x = end.x, y = end.y, z = end.z };
            // velocity = new Vec3 { x = 0, y = 0, z = 0 };
            
            logger.LogInformation(">>>>>>>>>>>>>>>>>>> CLOSE");
        }

        // Check for NaN values and handle them
        if (double.IsNaN(newPosition.x) || double.IsNaN(newPosition.y) || double.IsNaN(newPosition.z) ||
            double.IsNaN(velocity.x) || double.IsNaN(velocity.y) || double.IsNaN(velocity.z))
        {
            // Handle NaN case by setting position to end and stopping velocity
            newPosition = new Vec3 { x = end.x, y = end.y, z = end.z };
            // velocity = new Vec3 { x = 0, y = 0, z = 0 };
            
            logger.LogInformation("<<<<<<<<<<<<<<<<<<<<<< NAN");
        }

        return newPosition;
    }
}