using System;
using System.Threading.Tasks;
using Backend;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.NQ.Interfaces;
using NQ;
using NQutils;

namespace Mod.DynamicEncounters.Features.NQ.Services;

public class GameAlertService(IServiceProvider provider) : IGameAlertService
{
    public async Task PushInfoAlert(ulong playerId, string message)
    {
        var sanitizedMessage = message.Replace("\"", "\\\"");

        await provider.GetRequiredService<IPub>()
            .NotifyTopic(
                Topics.PlayerNotifications(playerId),
                new NQutils.Messages.ModTriggerHudEventRequest(
                    new ModTriggerHudEvent
                    {
                        eventName = "modinjectjs",
                        eventPayload = $"CPPHud.addSimpleNotification(\"{sanitizedMessage}\");",
                    }
                )
            );
    }

    public async Task PushErrorAlert(ulong playerId, string message)
    {
        var sanitizedMessage = message.Replace("\"", "\\\"");

        await provider.GetRequiredService<IPub>()
            .NotifyTopic(
                Topics.PlayerNotifications(playerId),
                new NQutils.Messages.ModTriggerHudEventRequest(
                    new ModTriggerHudEvent
                    {
                        eventName = "modinjectjs",
                        eventPayload = $"CPPHud.addFailureNotification(\"{sanitizedMessage}\");",
                    }
                )
            );
    }
}