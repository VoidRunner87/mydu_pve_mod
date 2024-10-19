using System;
using System.Threading.Tasks;
using Backend;
using Microsoft.Extensions.DependencyInjection;
using NQ;
using NQutils;

namespace Mod.DynamicEncounters.Features.Common.Services;

public class PlayerAlertService(IServiceProvider provider) : IPlayerAlertService
{
    public async Task SendErrorAlert(PlayerId playerId, string message)
    {
        var sanitizedMessage = message.Replace("\"", "\\\"");

        await provider.GetRequiredService<IPub>().NotifyTopic(
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

    public async Task SendInfoAlert(PlayerId playerId, string message)
    {
        var sanitizedMessage = message.Replace("\"", "\\\""); // Escape quotes if needed

        await provider.GetRequiredService<IPub>().NotifyTopic(
            Topics.PlayerNotifications(playerId),
            new NQutils.Messages.ModTriggerHudEventRequest(
                new ModTriggerHudEvent
                {
                    eventName = "modinjectjs",
                    eventPayload = $"CPPHud.addSimpleNotification(\"{sanitizedMessage}\");",
                }
            )
        );

        return;
    }

    public async Task SendNetworkNotification(PlayerId playerId, string message,
        int delay)
    {
        var sanitizedMessage = message.Replace("\"", "\\\""); // Escape quotes if needed

        // Send the notification to display the message
        await provider.GetRequiredService<IPub>().NotifyTopic(
            Topics.PlayerNotifications(playerId),
            new NQutils.Messages.ModTriggerHudEventRequest(
                new ModTriggerHudEvent
                {
                    eventName = "modinjectjs",
                    eventPayload = $"networkNotification.setMessage(\"{sanitizedMessage}\",10);",
                }
            )
        );

        // Add a delay to hide the message again
        await Task.Delay(delay);

        // Hide the message after the delay
        await provider.GetRequiredService<IPub>().NotifyTopic(
            Topics.PlayerNotifications(playerId),
            new NQutils.Messages.ModTriggerHudEventRequest(
                new ModTriggerHudEvent
                {
                    eventName = "modinjectjs",
                    eventPayload = $"networkNotification.show(false);",
                }
            )
        );
    }
}