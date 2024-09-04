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
using Orleans;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors;

public class SelectTargetBehavior(ulong constructId, IConstructDefinition constructDefinition) : IConstructBehavior
{
    private readonly IConstructDefinition _constructDefinition = constructDefinition;
    private bool _active = true;
    private IConstructSpatialHashRepository _spatialHashRepo;
    private IClusterClient _orleans;
    private ILogger<SelectTargetBehavior> _logger;

    public bool IsActive() => _active;

    public Task InitializeAsync(BehaviorContext context)
    {
        var provider = context.ServiceProvider;

        _spatialHashRepo = provider.GetRequiredService<IConstructSpatialHashRepository>();
        _orleans = provider.GetOrleans();
        _logger = provider.CreateLogger<SelectTargetBehavior>();
        
        return Task.CompletedTask;
    }

    public async Task TickAsync(BehaviorContext context)
    {
        if (!context.IsAlive)
        {
            _active = false;
            return;
        }

        var targetSpan = DateTime.UtcNow - context.TargetSelectedTime;
        if (targetSpan < TimeSpan.FromSeconds(20))
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

        foreach (var construct in playerConstructs)
        {
            if (counter > maxIterations)
            {
                break;
            }

            // Adds to the list of players involved
            if (construct.mutableData.pilot.HasValue)
            {
                context.PlayerIds.Add(construct.mutableData.pilot.Value.id);
            }

            var pos = construct.rData.position;

            var delta = pos.Distance(npcPos);
            if (delta < distance)
            {
                distance = delta;
                targetId = construct.rData.constructId;
            }

            counter++;
        }

        context.TargetConstructId = targetId == 0 ? null : targetId;
        context.TargetSelectedTime = DateTime.UtcNow;
        
        _logger.LogInformation("Selected a new Target: {Target}; {Time}ms", targetId, sw.ElapsedMilliseconds);
    }
}