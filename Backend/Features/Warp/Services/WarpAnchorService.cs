using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Backend;
using Backend.Database;
using Backend.Scenegraph;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Common.Helpers;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Common.Services;
using Mod.DynamicEncounters.Features.ExtendedProperties.Extensions;
using Mod.DynamicEncounters.Features.ExtendedProperties.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.TaskQueue.Interfaces;
using Mod.DynamicEncounters.Features.Warp.Data;
using Mod.DynamicEncounters.Features.Warp.Interfaces;
using Mod.DynamicEncounters.Helpers;
using Mod.DynamicEncounters.Vector.Helpers;
using NQ;
using NQ.Interfaces;
using NQutils.Def;
using NQutils.Sql;
using Orleans;

namespace Mod.DynamicEncounters.Features.Warp.Services;

public class WarpAnchorService(IServiceProvider provider) : IWarpAnchorService
{
    private readonly ILogger<WarpAnchorService> _logger = provider.CreateLogger<WarpAnchorService>();
    private readonly IPlayerAlertService _playerAlertService = provider.GetRequiredService<IPlayerAlertService>();
    private readonly IAreaScanService _areaScanService = provider.GetRequiredService<IAreaScanService>();

    private readonly IClusterClient _orleans = provider.GetRequiredService<IClusterClient>();

    public async Task<CreateWarpAnchorOutcome> SpawnWarpAnchor(SpawnWarpAnchorCommand command)
    {
        if (string.IsNullOrEmpty(command.ElementTypeName))
        {
            return CreateWarpAnchorOutcome.InvalidElementTypeName();
        }

        var spawner = provider.GetRequiredService<IBlueprintSpawnerService>();
        var sql = provider.GetRequiredService<ISql>();
        var taskQueueService = provider.GetRequiredService<ITaskQueueService>();
        var traitRepository = provider.GetRequiredService<ITraitRepository>();
        var elementTraitMap = (await traitRepository.GetElementTraits(command.ElementTypeName)).Map();

        if (!elementTraitMap.TryGetValue("supercruise", out var trait))
        {
            return CreateWarpAnchorOutcome.ElementDoesNotHaveSuperCruise(command.ElementTypeName);
        }

        trait.TryGetPropertyValue("blueprintFileName", out var blueprintFileName, "Warp_Signature.json");
        trait.TryGetPropertyValue("maxRange", out var maxRange, DistanceHelpers.OneSuInMeters * 100);

        var delta = command.TargetPosition - command.FromPosition;
        var distance = delta.Size();
        var direction = delta.NormalizeSafe();
        var beaconPosition = command.TargetPosition;

        if (distance > maxRange)
        {
            beaconPosition = direction * maxRange + command.FromPosition;
        }

        try
        {
            const string warpDestinationConstructName = "[!] Warp Signature";

            var constructId = await spawner.SpawnAsync(
                new SpawnArgs
                {
                    Folder = "pve",
                    File = blueprintFileName,
                    Position = beaconPosition,
                    IsUntargetable = true,
                    OwnerEntityId = new EntityId { playerId = command.PlayerId },
                    Name = warpDestinationConstructName
                }
            );

            var connectionFactory = provider.GetRequiredService<IPostgresConnectionFactory>();
            using var db = connectionFactory.Create();

            // Make sure the beacon is active by setting all elements to have been created 3 days in the past *shrugs*
            await db.ExecuteAsync(
                """
                UPDATE public.element SET created_at = NOW() - INTERVAL '3 DAYS' WHERE construct_id = @constructId
                """,
                new
                {
                    constructId = (long)constructId
                }
            );

            await taskQueueService.EnqueueScript(
                new ScriptActionItem
                {
                    Type = "reload-construct",
                    ConstructId = constructId
                },
                DateTime.UtcNow + TimeSpan.FromSeconds(60 + 50)
            );

            await taskQueueService.EnqueueScript(
                new ScriptActionItem
                {
                    Type = "delete",
                    ConstructId = constructId
                },
                DateTime.UtcNow + TimeSpan.FromMinutes(2)
            );

            await sql.UpdatePlayerProperty_Generic(
                command.PlayerId,
                "warpDestinationConstructName",
                new PropertyValue(warpDestinationConstructName)
            );

            await sql.UpdatePlayerProperty_Generic(
                command.PlayerId,
                "warpDestinationConstructId",
                new PropertyValue(constructId)
            );

            var beaconPosString = beaconPosition.Vec3ToPosition();

            await sql.UpdatePlayerProperty_Generic(
                command.PlayerId,
                "warpDestinationWorldPosition",
                new PropertyValue(beaconPosString)
            );

            return CreateWarpAnchorOutcome.WarpAnchorCreated(
                constructId, 
                warpDestinationConstructName,
                beaconPosition
            );
        }
        catch (Exception e)
        {
            return CreateWarpAnchorOutcome.Failed("Failed to create warp anchor", e);
        }
    }

    public async Task<CreateWarpAnchorOutcome> CreateWarpAnchorForPosition(CreateWarpAnchorCommand command)
    {
        const string pWarpAnchorTimePoint = "warpAnchorTimePoint";
        var playerId = command.PlayerId;

        var sql = provider.GetRequiredService<ISql>();
        var bank = provider.GetRequiredService<IGameplayBank>();
        var sceneGraph = provider.GetRequiredService<IScenegraph>();
        
        var propVal = await sql.ReadPlayerProperty_Generic(playerId, pWarpAnchorTimePoint);
        if (propVal is { value: not null })
        {
            var timePoint = new TimePoint { networkTime = propVal.intValue };
            var nowTimePoint = TimePoint.Now();

            var timeSpan = nowTimePoint.ToDateTime() - timePoint.ToDateTime();

            if (timeSpan < TimeSpan.FromMinutes(3))
            {
                var cooldownTime = timePoint.ToDateTime() + TimeSpan.FromMinutes(3);
                var remaining = nowTimePoint.ToDateTime() - cooldownTime;

                return CreateWarpAnchorOutcome.OnCooldown(remaining);
            }
        }

        var playerLocalPosition = await sceneGraph.GetPlayerLocalPosition(command.PlayerId);
        if (playerLocalPosition == null)
        {
            return CreateWarpAnchorOutcome.InvalidPlayerPosition();
        }

        var constructId = playerLocalPosition.constructId;
        var constructPos = await sceneGraph.GetConstructCenterWorldPosition(constructId);

        var constructGrain = _orleans.GetConstructGrain(constructId);
        var constructElementGrain = _orleans.GetConstructElementsGrain(constructId);

        var pilotId = await constructGrain.GetPilot();

        if (pilotId == null)
        {
            return CreateWarpAnchorOutcome.MustBePilotingConstruct();
        }

        var position = command.TargetPosition ?? new Vec3();

        if (!command.TargetPosition.HasValue)
        {
            var waypointPosString = await sql.ReadPlayerProperty(playerId, Character.d_currentWaypoint);

            if (waypointPosString == null || !waypointPosString.StartsWith("::pos{0,0"))
            {
                return CreateWarpAnchorOutcome.InvalidWaypoint();
            }

            _logger.LogInformation("Found Waypoint: {WP}", waypointPosString);

            try
            {
                position = waypointPosString.PositionToVec3();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Invalid Waypoint: {WP}", waypointPosString);
                return CreateWarpAnchorOutcome.InvalidWaypoint();
            }
        }

        var direction = (position - constructPos).Normalized();
        var offsetPos = direction * command.Offset;

        position += offsetPos;
        
        var contacts = await _areaScanService.ScanForPlanetaryBodies(position, 0.25d * DistanceHelpers.OneSuInMeters);
        if (contacts.Any())
        {
            return CreateWarpAnchorOutcome.TooCloseToAPlanet();
        }

        var warpDrives = await constructElementGrain.GetElementsOfType<WarpDriveUnit>();
        if (warpDrives.Count == 0)
        {
            return CreateWarpAnchorOutcome.MissingDriveUnit();
        }

        var driveUnitElementId = warpDrives.First();
        var driveUnitElementInfo = await constructElementGrain.GetElement(driveUnitElementId);
        var driveDef = bank.GetDefinition(driveUnitElementInfo.elementType);

        if (driveDef == null)
        {
            return CreateWarpAnchorOutcome.InvalidDriveUnit();
        }

        try
        {
            if (EnvironmentVariableHelper.IsProduction())
            {
                var propValue = await sql.ReadPlayerProperty_Generic(playerId, pWarpAnchorTimePoint);
                if (propValue?.value == null)
                {
                    await sql.SetPlayerProperties(playerId, new Dictionary<string, PropertyValue>
                    {
                        { pWarpAnchorTimePoint, new PropertyValue(TimePoint.Now().networkTime) }
                    });
                }
                else
                {
                    await sql.UpdatePlayerProperty_Generic(
                        playerId,
                        pWarpAnchorTimePoint,
                        new PropertyValue(TimePoint.Now().networkTime)
                    );
                }
            }
        }
        catch (Exception e)
        {
            await _playerAlertService.SendErrorAlert(
                playerId,
                "Failed to update warp anchor timer"
            );

            _logger.LogError(e, "Failed to update warp anchor timer");
        }

        try
        {
            return await SpawnWarpAnchor(
                new SpawnWarpAnchorCommand
                {
                    PlayerId = command.PlayerId,
                    FromPosition = constructPos,
                    TargetPosition = position,
                    ElementTypeName = driveDef.Name
                });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failure to Create Warp Anchor");

            return CreateWarpAnchorOutcome.Failed("Failure to Create Warp Anchor", e);
        }
    }

    public async Task<CreateWarpAnchorOutcome> CreateWarpAnchorForward(CreateWarpAnchorForwardCommand command)
    {
        var constructService = provider.GetRequiredService<IConstructService>();
        var sceneGraph = provider.GetRequiredService<IScenegraph>();

        var playerLocalPosition = await sceneGraph.GetPlayerLocalPosition(command.PlayerId);
        if (playerLocalPosition == null)
        {
            return CreateWarpAnchorOutcome.InvalidPlayerPosition();
        }

        var constructId = playerLocalPosition.constructId;

        var info = await constructService.GetConstructInfoAsync(constructId);
        var quat = info.Info!.rData.rotation.ToQuat();
        var pos = await sceneGraph.GetConstructCenterWorldPosition(constructId);

        var forward = Vector3.Transform(Vector3.UnitY, quat);
        var aheadPos = pos + forward.ToNqVec3() * command.Distance * DistanceHelpers.OneSuInMeters;

        return await CreateWarpAnchorForPosition(
            new CreateWarpAnchorCommand
            {
                PlayerId = command.PlayerId,
                TargetPosition = aheadPos,
            }
        );
    }

    public async Task<SetWarpCooldownOutcome> SetWarpCooldown(SetWarpCooldownCommand command)
    {
        var traitRepository = provider.GetRequiredService<ITraitRepository>();
        var elementTraitMap = (await traitRepository.GetElementTraits(command.ElementTypeName)).Map();

        if (!elementTraitMap.TryGetValue("supercruise", out var trait))
        {
            return SetWarpCooldownOutcome.NotASupercruiseDrive(command.ElementTypeName);
        }

        trait.TryGetPropertyValue("warpEndCooldown", out var warpEndCooldown, TimeSpan.FromSeconds(3).TotalMilliseconds);
        
        var orleans = provider.GetOrleans();

        var constructElementsGrain = orleans.GetConstructElementsGrain(command.ConstructId);
        var coreUnits = await constructElementsGrain.GetElementsOfType<CoreUnit>();

        if (coreUnits.Count == 0)
        {
            return SetWarpCooldownOutcome.InvalidConstruct();
        }

        var cooldownDate = DateTime.UtcNow + TimeSpan.FromSeconds(warpEndCooldown);
        
        await constructElementsGrain.UpdateElementProperty(new ElementPropertyUpdate
        {
            constructId = command.ConstructId,
            name = "endOfWarpCooldown",
            elementId = coreUnits.First().elementId,
            value = new PropertyValue(cooldownDate.ToNQTimePoint().networkTime),
            timePoint = TimePoint.Now()
        });

        return SetWarpCooldownOutcome.CooldownSet();
    }
}