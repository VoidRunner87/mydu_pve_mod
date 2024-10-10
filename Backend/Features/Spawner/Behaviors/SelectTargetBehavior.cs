using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Common.Vector;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Common.Services;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Services;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Features.Spawner.Extensions;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;
using NQutils.Def;
using Orleans;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors;

public class SelectTargetBehavior(ulong constructId, IPrefab prefab) : IConstructBehavior
{
    private bool _active = true;
    private IClusterClient _orleans;
    private ILogger<SelectTargetBehavior> _logger;
    private IConstructGrain _constructGrain;
    private IConstructService _constructService;
    private ISectorPoolManager _sectorPoolManager;

    public bool IsActive() => _active;

    public BehaviorTaskCategory Category => BehaviorTaskCategory.MediumPriority;

    public Task InitializeAsync(BehaviorContext context)
    {
        var provider = context.ServiceProvider;

        _orleans = provider.GetOrleans();
        _logger = provider.CreateLogger<SelectTargetBehavior>();
        _constructGrain = _orleans.GetConstructGrain(constructId);
        _constructService = provider.GetRequiredService<IConstructService>();
        _sectorPoolManager = provider.GetRequiredService<ISectorPoolManager>();

        return Task.CompletedTask;
    }

    public async Task TickAsync(BehaviorContext context)
    {
        if (!context.IsAlive)
        {
            _active = false;
            return;
        }
        
        var targetConstructId = context.GetTargetConstructId();

        var targetSpan = DateTime.UtcNow - context.TargetSelectedTime;
        if (targetSpan < TimeSpan.FromSeconds(10))
        {
            context.SetAutoTargetMovePosition(await GetTargetMovePosition(targetConstructId));

            return;
        }

        var sw = new Stopwatch();
        sw.Start();

        _logger.LogInformation("Construct {Construct} Selecting a new Target", constructId);

        if (!context.Position.HasValue)
        {
            var npcConstructInfo = await _constructService.NoCache().GetConstructInfoAsync(constructId);
            if (npcConstructInfo == null)
            {
                return;
            }

            context.Position = npcConstructInfo.rData.position;
        }

        var npcPos = context.Position.Value;

        var sectorPos = npcPos.GridSnap(SectorPoolManager.SectorGridSnap);
        var sectorGrid = new LongVector3(sectorPos);

        _logger.LogInformation("Construct {Construct} at Grid {Grid}", constructId, sectorGrid);

        var constructsOnSector = SectorGridConstructCache.FindAroundGrid(sectorGrid);

        var result = new List<ConstructInfo>();
        foreach (var id in constructsOnSector)
        {
            try
            {
                result.Add(await _constructService.GetConstructInfoAsync(id));
            }
            catch (Exception)
            {
                _logger.LogError("Failed to fetch construct info for {Construct}", id);
            }
        }

        var playerConstructs = result
            .Where(r => r.mutableData.ownerId.IsPlayer() || r.mutableData.ownerId.IsOrg())
            .ToList();

        _logger.LogInformation("Construct {Construct} Found {Count} PLAYER constructs around {List}. Time = {Time}ms",
            constructId,
            playerConstructs.Count,
            string.Join(", ", playerConstructs.Select(x => x.rData.constructId)),
            sw.ElapsedMilliseconds
        );

        ulong? targetId = null;
        var distance = double.MaxValue;
        const int maxIterations = 5;

        const long targetingDistance = 5 * DistanceHelpers.OneSuInMeters;

        foreach (var construct in playerConstructs.Take(maxIterations))
        {
            // Adds to the list of players involved
            if (construct.mutableData.pilot.HasValue)
            {
                context.PlayerIds.Add(construct.mutableData.pilot.Value.id);
            }

            var pos = construct.rData.position;

            var delta = Math.Abs(pos.Distance(npcPos));

            _logger.LogInformation("Construct {Construct} Distance: {Distance}su. Time = {Time}ms",
                construct.rData.constructId,
                delta / DistanceHelpers.OneSuInMeters,
                sw.ElapsedMilliseconds
            );

            if (delta > targetingDistance)
            {
                continue;
            }

            if (delta < distance)
            {
                distance = delta;
                targetId = construct.rData.constructId;
            }
        }

        if (targetId.HasValue && targetId.Value != 0)
        {
            context.SetAutoTargetConstructId(targetId);
        }

        if (!targetConstructId.HasValue)
        {
            return;
        }

        var returnToSector = false;
        if (context.Position.HasValue)
        {
            var targetConstructInfo = await _constructService.GetConstructInfoAsync(targetConstructId.Value);
            if (targetConstructInfo != null)
            {
                var targetPos = targetConstructInfo.rData.position;

                var targetDistance = (targetPos - context.Position.Value).Size();
                if (targetDistance > 10 * DistanceHelpers.OneSuInMeters)
                {
                    returnToSector = true;
                }
            }

            if (await _constructService.IsInSafeZone(targetConstructId.Value))
            {
                returnToSector = true;
            }
        }

        if (returnToSector)
        {
            _logger.LogInformation("Construct {Construct} Returning to Sector", constructId);
        }

        var targetMovePositionTask = GetTargetMovePosition(targetConstructId);
        var cacheTargetElementPositionsTask = CacheTargetElementPositions(context, targetConstructId);

        await Task.WhenAll([targetMovePositionTask, cacheTargetElementPositionsTask]);

        if (returnToSector)
        {
            context.SetAutoTargetMovePosition(context.Sector);
        }
        else
        {
            context.SetAutoTargetMovePosition(await targetMovePositionTask);

            await _sectorPoolManager.SetExpirationFromNow(context.Sector, TimeSpan.FromHours(1));
        }

        if (targetId.HasValue)
        {
            _logger.LogInformation("Selected a new Target: {Target}; {Time}ms", targetId, sw.ElapsedMilliseconds);
        }

        try
        {
            var npcConstructInfo = await _constructService.GetConstructInfoAsync(constructId);
            if (npcConstructInfo == null)
            {
                return;
            }

            var constructInfo = await _constructService.GetConstructInfoAsync(targetConstructId.Value);
            if (constructInfo == null)
            {
                return;
            }

            var targeting = new TargetingConstructData
            {
                constructId = constructId,
                ownerId = new EntityId { playerId = prefab.DefinitionItem.OwnerId },
                constructName = npcConstructInfo.rData.name
            };

            await _constructService.SendIdentificationNotification(
                targetConstructId.Value,
                targeting
            );

            await _constructService.SendAttackingNotification(
                targetConstructId.Value,
                targeting
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to Identity Target");
        }

        try
        {
            await PilotingTakeOverAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to Takeover Ship");
        }
    }

    private async Task CacheTargetElementPositions(BehaviorContext context, ulong? targetConstructId)
    {
        if (!targetConstructId.HasValue)
        {
            return;
        }

        var constructElementsGrain = _orleans.GetConstructElementsGrain(targetConstructId.Value);
        var elements = (await constructElementsGrain.GetElementsOfType<ConstructElement>()).ToList();

        var elementInfoListTasks = elements
            .Select(constructElementsGrain.GetElement);

        var elementInfoList = await Task.WhenAll(elementInfoListTasks);
        context.TargetElementPositions = elementInfoList.Select(x => x.position);
    }

    private async Task PilotingTakeOverAsync()
    {
        if (!await _constructService.IsBeingControlled(constructId))
        {
            await _constructGrain.PilotingTakeOver(ModBase.Bot.PlayerId, true);
        }
    }

    private async Task<Vec3> GetTargetMovePosition(ulong? targetConstructId)
    {
        if (!targetConstructId.HasValue)
        {
            return new Vec3();
        }

        var targetConstructInfo = await _constructService.GetConstructInfoAsync(targetConstructId.Value);
        if (targetConstructInfo == null)
        {
            _logger.LogError(
                "Construct {Construct} Target construct info {Target} is null", constructId,
                targetConstructId.Value
            );
            return new Vec3();
        }

        var targetPos = targetConstructInfo.rData.position;

        var distanceGoal = prefab.DefinitionItem.TargetDistance;
        var offset = new Vec3 { y = distanceGoal };

        return targetPos + offset;
    }
}