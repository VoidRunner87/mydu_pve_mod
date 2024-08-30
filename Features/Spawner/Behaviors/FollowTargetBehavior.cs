using System.Threading.Tasks;
using BotLib.Generated;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;
using Orleans;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors;

public class FollowTargetBehavior(ulong constructId, IConstructDefinition constructDefinition) : IConstructBehavior
{
    private TimePoint _timePoint = new();
    private IClusterClient _orleans;
    private readonly IConstructDefinition _constructDefinition = constructDefinition;

    private bool _active = true;

    public bool IsActive() => _active;

    public Task InitializeAsync(BehaviorContext context)
    {
        var provider = context.ServiceProvider;
        _orleans = provider.GetOrleans();
        
        return Task.CompletedTask;
    }

    public async Task TickAsync(BehaviorContext context)
    {
        if (!context.IsAlive)
        {
            _active = false;
            
            return;
        }

        if (!context.TargetConstructId.HasValue)
        {
            return;
        }
        
        var provider = context.ServiceProvider;
        var orleans = provider.GetOrleans();
        var client = context.Client;

        var targetConstructInfoGrain = orleans.GetConstructInfoGrain(context.TargetConstructId.Value);
        var targetConstructInfo = await targetConstructInfoGrain.Get();

        var npcConstructInfoGrain = orleans.GetConstructInfoGrain(constructId);
        var npcConstructInfo = await npcConstructInfoGrain.Get();

        var direction = (targetConstructInfo.rData.position - npcConstructInfo.rData.position + new Vec3{y = 20000}).Normalized();
        var velocity = direction * 15 * 9.81f;

        var rotation = VectorMathHelper.CalculateRotationToPoint(
            npcConstructInfo.rData.position,
            direction
        );

        context.Velocity += velocity;
        context.Velocity = context.Velocity.ClampToSize(20000 / 3.6d);

        var finalVelocity = context.Velocity * context.DeltaTime;
        
        _timePoint.networkTime++;
        
        await client.Req.ConstructUpdate(
            new ConstructUpdate
            {
                constructId = constructId,
                rotation = rotation,
                position = npcConstructInfo.rData.position + finalVelocity,
                worldAbsoluteVelocity = new Vec3(),
                worldAbsoluteAngVelocity = new Vec3(),
                worldRelativeAngVelocity = new Vec3(),
                worldRelativeVelocity = finalVelocity,
                time = _timePoint,
                grounded = false,
            }
        );
    }
}