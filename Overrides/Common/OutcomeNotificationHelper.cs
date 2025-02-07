using System;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Overrides.ApiClient.Data;

namespace Mod.DynamicEncounters.Overrides.Common;

public static class OutcomeNotificationHelper
{
    public static async Task NotifyPlayer(this BasicOutcome outcome, IServiceProvider provider, ulong playerId)
    {
        if (outcome.Success)
        {
            await Notifications.SimpleNotificationToPlayer(provider, playerId, outcome.Message);
        }
        else
        {
            await Notifications.ErrorNotification(provider, playerId, outcome.Message);
        }
    }
}