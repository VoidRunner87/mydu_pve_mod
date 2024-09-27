using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Helpers;
using Mod.DynamicEncounters.Helpers.DU;
using Orleans;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors;

public class RetreatBehavior(ulong constructId, IPrefab prefab) : IConstructBehavior
{
    private readonly IPrefab _prefab = prefab;
    private ILogger<RetreatBehavior> _logger;
    private IClusterClient _orleans;
    private IConstructService _constructService;

    public Task InitializeAsync(BehaviorContext context)
    {
        var provider = context.ServiceProvider;
        _orleans = provider.GetOrleans();
        _constructService = provider.GetRequiredService<IConstructService>();
        _logger = provider.CreateLogger<RetreatBehavior>();

        return Task.CompletedTask;
    }

    public async Task TickAsync(BehaviorContext context)
    {
        if (!context.IsAlive)
        {
            return;
        }
        
        var npcInfo = await _constructService.GetConstructInfoAsync(constructId);

        if (npcInfo == null)
        {
            return;
        }

        // first time initialize position
        if (!context.Position.HasValue)
        {
            context.Position = npcInfo.rData.position;
        }
        
        var npcPos = context.Position.Value;
        
        if (!context.TargetConstructId.HasValue)
        {
            context.TargetMovePosition = context.Sector;
            return;
        }
        
        var targetConstructInfo = await _constructService.GetConstructInfoAsync(context.TargetConstructId.Value);

        if (targetConstructInfo == null)
        {
            context.TargetMovePosition = context.Sector;
            return;
        }
        
        var targetPos = targetConstructInfo.rData.position;

        if (!npcInfo.HasShield())
        {
            return;
        }
        
        if (npcInfo.IsShieldLowerThan25() || npcInfo.IsShieldDown())
        {
            var retreatDirection = (npcPos - targetPos).NormalizeSafe();
            var npcVel = await _constructService.GetConstructVelocities(constructId);
            var targetVel = await _constructService.GetConstructVelocities(context.TargetConstructId.Value);

            var npcVelDir = npcVel.Linear.NormalizeSafe();
            var targetVelDir = targetVel.Linear.NormalizeSafe();
            
            var alreadySomewhatFast = npcVel.Linear.Size() > 10000;
            var oppositeVelocities = npcVelDir.Dot(targetVelDir) < -0.4;

            // _logger.LogInformation("Dot {Dot} | {VelLen}", npcVelDir.Dot(targetVelDir), npcVel.Linear.Size());
            
            // if (oppositeVelocities)
            // {   
            //     retreatDirection *= -1; // reverse
            // }
            
            context.TargetMovePosition = npcPos + retreatDirection * 2.5 * DistanceHelpers.OneSuInMeters;
        }

        var isPrettyFar = Math.Abs(targetPos.Dist(npcPos)) > 1.7 * DistanceHelpers.OneSuInMeters;
        
        var shouldVentShields = npcInfo.IsShieldDown() || 
                                (npcInfo.IsShieldLowerThan25() && isPrettyFar) ||
                                (npcInfo.IsShieldLowerThanHalf() && isPrettyFar);

        context.TryGetProperty("ShieldVentTimer", out var shieldVentTimer, 0d);
        shieldVentTimer += context.DeltaTime;
        
        if (shouldVentShields)
        {
            if (shieldVentTimer > 5)
            {
                await _constructService.TryVentShieldsAsync(constructId);
                shieldVentTimer = 0;
            }

            context.SetProperty("ShieldVentTimer", shieldVentTimer);
        }
    }
}