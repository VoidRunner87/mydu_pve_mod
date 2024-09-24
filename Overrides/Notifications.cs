using System;
using System.Threading.Tasks;
using Backend;
using Microsoft.Extensions.DependencyInjection;
using NQutils;

namespace Mod.DynamicEncounters.Overrides;

public class Notifications
{
    public static async Task ErrorNotification(IServiceProvider provider, NQ.PlayerId pid, string message)
    {
        var sanitizedMessage = message.Replace("\"", "\\\"");

        await provider.GetRequiredService<IPub>().NotifyTopic(
            Topics.PlayerNotifications(pid),
            new NQutils.Messages.ModTriggerHudEventRequest(
                new NQ.ModTriggerHudEvent
                {
                    eventName = "modinjectjs",
                    eventPayload = $"CPPHud.addFailureNotification(\"{sanitizedMessage}\");",
                }
            )
        );
    }

    public static async Task NetworkNotification(IServiceProvider provider, NQ.PlayerId pid, string message, int delay)
    {
        var sanitizedMessage = message.Replace("\"", "\\\""); // Escape quotes if needed

        // Send the notification to display the message
        await provider.GetRequiredService<IPub>().NotifyTopic(
            Topics.PlayerNotifications(pid),
            new NQutils.Messages.ModTriggerHudEventRequest(
                new NQ.ModTriggerHudEvent
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
            Topics.PlayerNotifications(pid),
            new NQutils.Messages.ModTriggerHudEventRequest(
                new NQ.ModTriggerHudEvent
                {
                    eventName = "modinjectjs",
                    eventPayload = $"networkNotification.show(false);",
                }
            )
        );
    }

    // posx and posy will possition the notification on screen
    // duration is in seconds so 5 will mean 5 seconds
    public static async Task HintNotification(
        IServiceProvider provider,
        NQ.PlayerId pid,
        string header,
        string body,
        string footer,
        int posx,
        int posy,
        int duration
    )
    {
        // Escape quotes in the message strings if necessary
        var sanitizedHeader = header.Replace("\"", "\\\"");
        var sanitizedBody = body.Replace("\"", "\\\"");
        var sanitizedFooter = footer.Replace("\"", "\\\"");

        // callable from debug panel : hintNotification.show('{"header":"Test Header","body":"This is the body of the notification.","footer":"Footer text here.","posx":1400,"posy":200,"duration":5}');
        // Construct the JSON payload for hintNotification.show
        var hintNotificationPayload =
            $"{{\"header\":\"{sanitizedHeader}\",\"body\":\"{sanitizedBody}\",\"footer\":\"{sanitizedFooter}\",\"posx\":{posx},\"posy\":{posy},\"duration\":{duration}}}";

        // Send the notification to display the hint
        await provider.GetRequiredService<IPub>().NotifyTopic(
            Topics.PlayerNotifications(pid),
            new NQutils.Messages.ModTriggerHudEventRequest(
                new NQ.ModTriggerHudEvent
                {
                    eventName = "modinjectjs",
                    eventPayload = $"hintNotification.show('{hintNotificationPayload}');"
                }
            )
        );
    }
}