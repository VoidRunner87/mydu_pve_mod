using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Services;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;
using NQutils.Def;
using Orleans;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors;

public class SelectTargetBehavior(ulong constructId, IPrefab prefab) : IConstructBehavior
{
    private readonly IPrefab _prefab = prefab;
    private bool _active = true;
    private IConstructSpatialHashRepository _spatialHashRepo;
    private IClusterClient _orleans;
    private ILogger<SelectTargetBehavior> _logger;
    private IConstructElementsGrain _constructElementsGrain;
    private IConstructGrain _constructGrain;

    public bool IsActive() => _active;

    public async Task InitializeAsync(BehaviorContext context)
    {
        var provider = context.ServiceProvider;

        _spatialHashRepo = provider.GetRequiredService<IConstructSpatialHashRepository>();
        _orleans = provider.GetOrleans();
        _logger = provider.CreateLogger<SelectTargetBehavior>();
        _constructElementsGrain = _orleans.GetConstructElementsGrain(constructId);
        _constructGrain = _orleans.GetConstructGrain(constructId);

        var radarElementId = (await _constructElementsGrain.GetElementsOfType<RadarPvPSpace>()).FirstOrDefault();
        var gunnerSeatElementId = (await _constructElementsGrain.GetElementsOfType<PVPSeatUnit>()).FirstOrDefault();

        context.ExtraProperties.TryAdd("RADAR_ID", radarElementId.elementId);
        context.ExtraProperties.TryAdd("SEAT_ID", gunnerSeatElementId.elementId);
    }

    public async Task TickAsync(BehaviorContext context)
    {
        if (!context.IsAlive)
        {
            _active = false;
            return;
        }

        var targetSpan = DateTime.UtcNow - context.TargetSelectedTime;
        if (targetSpan < TimeSpan.FromSeconds(10))
        {
            return;
        }

        var sw = new Stopwatch();
        sw.Start();

        _logger.LogInformation("Selecting a new Target");

        var npcConstructInfoGrain = _orleans.GetConstructInfoGrain(constructId);
        var npcConstructInfo = await npcConstructInfoGrain.Get();
        var npcPos = npcConstructInfo.rData.position;
        var sectorPos = npcPos.GridSnap(SectorPoolManager.SectorGridSnap);

        var constructsOnSector = await _spatialHashRepo.FindPlayerLiveConstructsOnSector(sectorPos);

        var result = new List<ConstructInfo>();
        foreach (var id in constructsOnSector)
        {
            try
            {
                result.Add(await _orleans.GetConstructInfoGrain(id).Get());
            }
            catch (Exception)
            {
                _logger.LogError("Failed to fetch construct info for {Construct}", id);
            }
        }

        _logger.LogInformation("Found {Count} constructs around", result.Count);

        // TODO remove hardcoded
        var playerConstructs = result
            .Where(r => r.mutableData.ownerId.IsPlayer() || r.mutableData.ownerId.IsOrg())
            .ToList();

        _logger.LogInformation("Found {Count} PLAYER constructs around {List}",
            playerConstructs.Count,
            string.Join(", ", playerConstructs.Select(x => x.rData.constructId))
        );

        ulong targetId = 0;
        var distance = double.MaxValue;
        int maxIterations = 10;
        int counter = 0;

        var targetingDistance = 5 * 200000;

        foreach (var construct in playerConstructs)
        {
            if (counter > maxIterations)
            {
                break;
            }

            // Adds to the list of players involved
            if (construct.mutableData.pilot.HasValue)
            {
                context.PlayerIds.TryAdd(
                    construct.mutableData.pilot.Value.id,
                    construct.mutableData.pilot.Value.id
                );
            }
            
            var pos = construct.rData.position;

            var delta = Math.Abs(pos.Distance(npcPos));

            _logger.LogInformation("Construct {Construct} Distance: {Distance}m", construct.rData.constructId, delta);

            if (delta > targetingDistance)
            {
                continue;
            }

            if (delta < distance)
            {
                distance = delta;
                targetId = construct.rData.constructId;
            }

            counter++;
        }

        context.TargetConstructId = targetId == 0 ? null : targetId;
        context.TargetSelectedTime = DateTime.UtcNow;

        if (!context.TargetConstructId.HasValue)
        {
            return;
        }

        var constructElementsGrain = _orleans.GetConstructElementsGrain(context.TargetConstructId.Value);
        var elements = (await constructElementsGrain.GetElementsOfType<ConstructElement>()).ToList();
        
        var elementInfoListTasks = elements
            .Select(constructElementsGrain.GetElement);

        var elementInfoList = await Task.WhenAll(elementInfoListTasks);
        context.TargetElementPositions = elementInfoList.Select(x => x.position);
        
        _logger.LogInformation("Selected a new Target: {Target}; {Time}ms", targetId, sw.ElapsedMilliseconds);

        if (!context.ExtraProperties.TryGetValue<ulong>("RADAR_ID", out var radarElementId))
        {
            return;
        }

        if (!context.ExtraProperties.TryGetValue<ulong>("SEAT_ID", out var seatElementId))
        {
            return;
        }

        try
        {
            var constructInfoGrain = _orleans.GetConstructInfoGrain(context.TargetConstructId.Value);
            var constructInfo = await constructInfoGrain.Get();

            await _orleans.GetRadarGrain(radarElementId)
                .IdentifyStart(ModBase.Bot.PlayerId, new RadarIdentifyTarget
                {
                    playerId = constructInfo.mutableData.pilot ?? 0,
                    sourceConstructId = constructId,
                    targetConstructId = context.TargetConstructId.Value,
                    sourceRadarElementId = radarElementId,
                    sourceSeatElementId = seatElementId
                });

            await _constructGrain.PilotingTakeOver(ModBase.Bot.PlayerId, true);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to Identity Target");
        }
    }
}