using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Backend.Scenegraph;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Commands.Interfaces;
using Mod.DynamicEncounters.Features.Common.Services;
using Mod.DynamicEncounters.Features.Interfaces;
using Mod.DynamicEncounters.Features.Warp.Data;
using Mod.DynamicEncounters.Features.Warp.Interfaces;
using Mod.DynamicEncounters.Helpers;
using Mod.DynamicEncounters.Vector.Helpers;
using NQ;
using NQ.Interfaces;

namespace Mod.DynamicEncounters.Features.Commands.Services;

public partial class WarpAnchorCommandHandler : IWarpAnchorCommandHandler
{
    private readonly ILogger<NpcKillsCommandHandler> _logger =
        ModBase.ServiceProvider.CreateLogger<NpcKillsCommandHandler>();

    private readonly IFeatureReaderService _featureReaderService =
        ModBase.ServiceProvider.GetRequiredService<IFeatureReaderService>();

    private readonly IModManagerGrain _modManagerGrain = ModBase.ServiceProvider.GetOrleans().GetModManagerGrain();
    private readonly IScenegraph _sceneGraph = ModBase.ServiceProvider.GetRequiredService<IScenegraph>();

    private readonly IPlayerAlertService _playerAlertService =
        ModBase.ServiceProvider.GetRequiredService<IPlayerAlertService>();

    public async Task HandleCommand(ulong instigatorPlayerId, string command)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            { nameof(instigatorPlayerId), instigatorPlayerId },
            { nameof(command), command }
        });

        if (command == "@wac")
        {
            await HandleCreateWarpAnchorCommand(instigatorPlayerId);
        }
        else if (command.StartsWith("@wac ::pos{0,0,"))
        {
            var pieces = command.Split(" ");
            var position = pieces[1];

            try
            {
                var posVec = position.PositionToVec3();

                var warpAnchorService = ModBase.ServiceProvider.GetRequiredService<IWarpAnchorService>();
                var outcome = await warpAnchorService.CreateWarpAnchorForPosition(
                    new CreateWarpAnchorCommand
                    {
                        TargetPosition = posVec,
                        PlayerId = instigatorPlayerId
                    }
                );

                await SendAlertForOutcome(instigatorPlayerId, outcome);
            }
            catch (Exception e)
            {
                await _playerAlertService.SendErrorAlert(instigatorPlayerId, "Failed to process command. Invalid position");
                _logger.LogError(e, "Failed to process wac position command");
            }
        }
        else if (MatchesWarpAnchorForwardCommand().IsMatch(command))
        {
            var pieces = command.Split(" ");
            if (!double.TryParse(pieces[1], out var distance))
            {
                await SendAlertForOutcome(instigatorPlayerId, CreateWarpAnchorOutcome.InvalidDistance());
                return;
            }

            const double su12KmDistance = 12000D / DistanceHelpers.OneSuInMeters;
            
            var warpAnchorService = ModBase.ServiceProvider.GetRequiredService<IWarpAnchorService>();
            var outcome = await warpAnchorService.CreateWarpAnchorForward(
                new CreateWarpAnchorForwardCommand
                {
                    Distance = distance + su12KmDistance,
                    PlayerId = instigatorPlayerId
                }
            );
            
            await SendAlertForOutcome(instigatorPlayerId, outcome);
        }
    }

    private async Task SendAlertForOutcome(ulong instigatorPlayerId, CreateWarpAnchorOutcome outcome)
    {
        if (outcome.Success)
        {
            await _playerAlertService.SendInfoAlert(instigatorPlayerId, outcome.Message);
        }
        else
        {
            await _playerAlertService.SendErrorAlert(instigatorPlayerId, outcome.Message);
        }
    }

    private async Task HandleCreateWarpAnchorCommand(ulong instigatorPlayerId)
    {
        var warpAnchorModActionId = await _featureReaderService.GetIntValueAsync("WarpAnchorActionId", 3);
        var warpAnchorModName =
            await _featureReaderService.GetStringValueAsync("WarpAnchorModName", "Mod.DynamicEncounters");

        var (local, _) = await _sceneGraph.GetPlayerWorldPosition(instigatorPlayerId);

        if (local.constructId <= 0)
        {
            await _playerAlertService.SendErrorAlert(instigatorPlayerId, "You need to be on a construct");
            return;
        }

        var constructGrain = ModBase.ServiceProvider.GetOrleans().GetConstructGrain(local.constructId);
        var pilot = await constructGrain.GetPilot();

        if (pilot != instigatorPlayerId)
        {
            await _playerAlertService.SendErrorAlert(instigatorPlayerId, "You need to pilot the construct");
            return;
        }

        await _modManagerGrain.TriggerModAction(
            instigatorPlayerId,
            new ModAction
            {
                modName = warpAnchorModName,
                actionId = (ulong)warpAnchorModActionId,
                constructId = local.constructId
            }
        );
    }

    [GeneratedRegex("@wac [0-9]+")]
    private static partial Regex MatchesWarpAnchorForwardCommand();
}