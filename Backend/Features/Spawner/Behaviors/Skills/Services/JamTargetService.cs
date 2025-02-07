using System;
using System.Threading.Tasks;
using Backend;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Services;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Data;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;
using NQutils.Def;
using ConstructDisappear = NQutils.Messages.ConstructDisappear;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Services;

public class JamTargetService(IServiceProvider provider) : IJamTargetService
{
    public async Task<JamTargetOutcome> JamAsync(JamConstructCommand command)
    {
        var orleans = provider.GetOrleans();
        var pub = provider.GetRequiredService<IPub>();
        var alertService = provider.GetRequiredService<IPlayerAlertService>();

        var instigatorConstructId = command.InstigatorConstructId;
        var targetConstructId = command.TargetConstructId;

        var constructElementsGrain = orleans.GetConstructElementsGrain(targetConstructId);
        var radars = await constructElementsGrain.GetElementsOfType<RadarPVPUnit>();

        var targetConstructGrain = orleans.GetConstructGrain(targetConstructId);
        var pilot = await targetConstructGrain.GetPilot();

        var constructInfoGrain = orleans.GetConstructInfoGrain(instigatorConstructId);
        var info = await constructInfoGrain.Get();

        if (!pilot.HasValue)
        {
            return JamTargetOutcome.FailedTargetWithoutPilot();
        }

        foreach (var radar in radars)
        {
            var radarCamera = new CameraId { id = radar.elementId, kind = CameraKind.Radar };
            await pub.NotifyPlayer(pilot.Value, new ConstructDisappear(
                new global::NQ.ConstructDisappear
                {
                    constructId = instigatorConstructId,
                    camera = radarCamera
                }));

            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(command.DurationSeconds));
                await pub.NotifyPlayer(pilot.Value, new NQutils.Messages.ConstructAppear(
                    new ConstructAppear
                    {
                        camera = radarCamera,
                        info = info
                    }));
            });
        }

        if (command.SendAlert)
        {
            await alertService.SendErrorAlert(pilot.Value, "You are being jammed");
        }

        return JamTargetOutcome.Jammed();
    }
}