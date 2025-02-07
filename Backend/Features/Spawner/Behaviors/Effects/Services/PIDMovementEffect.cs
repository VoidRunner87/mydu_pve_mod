using Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Services;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Helpers;
using NQ;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Services;

public class PIDMovementEffect : IMovementEffect
{
    public static double Kp { get; set; } = 0.2d; 
    public static double Kd { get; set; } = 0.3d; 
    public static double Ki { get; set; } = 0d; 
    
    public IMovementEffect.Outcome Move(IMovementEffect.Params @params, BehaviorContext context)
    {
        var deltaTime = @params.DeltaTime;
        var npcVelocity = @params.Velocity;
        var npcPosition = @params.Position;
        var playerPosition = @params.TargetPosition;
        var maxAcceleration = @params.MaxAcceleration;
        var maxSpeed = @params.MaxVelocity;
        var deadZone = 1.0;
        var brakingThreshold = 100000;
        
        var pid = new PIDController(Kp, Ki, Kd);

        // Compute desired acceleration using PID
        Vec3 desiredAcceleration = pid.Compute(npcPosition, playerPosition, deltaTime, deadZone);

        // Clamp the acceleration to the max allowable value
        desiredAcceleration = desiredAcceleration.ClampToSize(maxAcceleration);

        // Apply braking phase near the target
        double distanceToTarget = (playerPosition - npcPosition).Size();
        if (distanceToTarget < brakingThreshold)
        {
            desiredAcceleration = npcVelocity.NormalizeSafe().Reverse() * maxAcceleration;
        }

        // Update NPC velocity based on acceleration
        npcVelocity = new Vec3
        {
            x = npcVelocity.x + desiredAcceleration.x * deltaTime,
            y = npcVelocity.y + desiredAcceleration.y * deltaTime,
            z = npcVelocity.z + desiredAcceleration.z * deltaTime
        };

        // Clamp velocity to the maximum speed
        npcVelocity = npcVelocity.ClampToSize(maxSpeed);

        // Update NPC position based on velocity
        npcPosition = new Vec3
        {
            x = npcPosition.x + npcVelocity.x * deltaTime,
            y = npcPosition.y + npcVelocity.y * deltaTime,
            z = npcPosition.z + npcVelocity.z * deltaTime
        };

        return new IMovementEffect.Outcome
        {
            Position = npcPosition,
            Velocity = npcVelocity
        };
    }
}