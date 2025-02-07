using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Events.Data;
using Mod.DynamicEncounters.Features.Events.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Features.Spawner.Extensions;

public static class BehaviorContextNotificationExtensions
{
    public static async Task NotifyShieldHpHalfAsync(this BehaviorContext context, BehaviorEventArgs eventArgs)
    {
        if (context.PublishedEvents.ContainsKey(nameof(NotifyShieldHpHalfAsync)))
        {
            return;
        }

        await context.Prefab.Events.OnShieldHalfAction.ExecuteAsync(
            new ScriptContext(
                eventArgs.Context.Provider,
                eventArgs.Context.FactionId,
                eventArgs.Context.PlayerIds,
                eventArgs.Context.Sector,
                eventArgs.Context.TerritoryId
            )
            {
                ConstructId = eventArgs.ConstructId,
            }
        );

        context.PublishedEvents.TryAdd(nameof(NotifyShieldHpHalfAsync), true);
    }

    public static async Task NotifyShieldHpLowAsync(this BehaviorContext context, BehaviorEventArgs eventArgs)
    {
        if (context.PublishedEvents.ContainsKey(nameof(NotifyShieldHpLowAsync)))
        {
            return;
        }

        await context.Prefab.Events.OnShieldLowAction.ExecuteAsync(
            new ScriptContext(
                eventArgs.Context.Provider,
                eventArgs.Context.FactionId,
                eventArgs.Context.PlayerIds,
                eventArgs.Context.Sector,
                eventArgs.Context.TerritoryId
            )
            {
                ConstructId = eventArgs.ConstructId
            }
        );

        context.PublishedEvents.TryAdd(nameof(NotifyShieldHpLowAsync), true);
    }

    public static async Task NotifyShieldHpDownAsync(this BehaviorContext context, BehaviorEventArgs eventArgs)
    {
        if (context.PublishedEvents.ContainsKey(nameof(NotifyShieldHpDownAsync)))
        {
            return;
        }

        await context.Prefab.Events.OnShieldDownAction.ExecuteAsync(
            new ScriptContext(
                eventArgs.Context.Provider,
                eventArgs.Context.FactionId,
                eventArgs.Context.PlayerIds,
                eventArgs.Context.Sector,
                eventArgs.Context.TerritoryId
            )
            {
                ConstructId = eventArgs.ConstructId
            }
        );

        context.PublishedEvents.TryAdd(nameof(NotifyShieldHpDownAsync), true);
    }

    public static async Task NotifyCoreStressHighAsync(this BehaviorContext context, BehaviorEventArgs eventArgs)
    {
        if (context.PublishedEvents.ContainsKey(nameof(NotifyCoreStressHighAsync)))
        {
            return;
        }

        await context.Prefab.Events.OnCoreStressHigh.ExecuteAsync(
            new ScriptContext(
                eventArgs.Context.Provider,
                eventArgs.Context.FactionId,
                eventArgs.Context.PlayerIds.ToHashSet(),
                eventArgs.Context.Sector,
                eventArgs.Context.TerritoryId
            )
            {
                ConstructId = eventArgs.ConstructId
            }
        );

        context.PublishedEvents.TryAdd(nameof(NotifyCoreStressHighAsync), true);
    }

    public static async Task NotifyConstructDestroyedAsync(this BehaviorContext context, BehaviorEventArgs eventArgs)
    {
        if (context.PublishedEvents.ContainsKey(nameof(NotifyConstructDestroyedAsync)))
        {
            return;
        }

        var eventService = context.Provider.GetRequiredService<IEventService>();

        var taskList = new List<Task>();

        // send event for all players piloting constructs
        // TODO #limitation = not considering gunners and boarders
        var logger = eventArgs.Context.Provider.CreateLogger<BehaviorContext>();

        var targetConstructId = eventArgs.Context.GetTargetConstructId();

        var constructService = eventArgs.Context.Provider
            .GetRequiredService<IConstructService>();

        try
        {
            if (eventArgs.Context.PlayerIds.Count == 0)
            {
                if (targetConstructId.HasValue)
                {
                    logger.LogWarning("Could not find any players. Fallback logic will use target construct owner");

                    var constructInfoOutcome = await constructService.NoCache()
                        .GetConstructInfoAsync(targetConstructId.Value);
                    var constructInfo = constructInfoOutcome.Info;

                    if (constructInfo?.mutableData.pilot != null)
                    {
                        var playerId = constructInfo.mutableData.pilot.Value;
                        eventArgs.Context.PlayerIds.Add(playerId);

                        logger.LogWarning("Found Player({Player}) on NOCACHE attempt", playerId);
                    }
                    else if (constructInfo != null && eventArgs.Context.PlayerIds.Count == 0)
                    {
                        var owner = constructInfo.mutableData.ownerId;

                        if (owner.IsPlayer())
                        {
                            eventArgs.Context.PlayerIds.Add(owner.playerId);
                            logger.LogWarning("Found Player({Player}) OWNER", owner.playerId);
                        }
                        else
                        {
                            logger.LogError("Owner is an Organization({Org}). This is not handled yet.",
                                owner.organizationId);
                        }
                    }
                }
                else
                {
                    logger.LogError("Can't use fallback - no target construct");
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to give quanta to Target Construct ({Construct}) Pilot", targetConstructId);
        }

        logger.LogInformation("NPC Defeated by players: {Players}", string.Join(", ", eventArgs.Context.PlayerIds));

        var tasks = eventArgs.Context.PlayerIds.Select(id =>
            eventService.PublishAsync(
                new PlayerDefeatedNpcEvent(
                    id,
                    eventArgs.Context.Sector,
                    eventArgs.ConstructId,
                    eventArgs.Context.FactionId,
                    eventArgs.Context.PlayerIds.Count
                )
            )
        );

        taskList.AddRange(tasks);

        var totalDamage = context.GetTotalDamageFromHistory();
        var totalDamageByPlayer = context.GetTotalDamageByPlayer();
        var playerWithMinDamage = totalDamageByPlayer
            .Where(kvp => kvp.Value > totalDamage * 0.1d)
            .Select(kvp => kvp.Key)
            .ToHashSet();

        if (playerWithMinDamage.Count == 0)
        {
            var topDamagePlayers = totalDamageByPlayer.OrderByDescending(x => x.Value);
            playerWithMinDamage = topDamagePlayers.Take(5).Select(x => x.Key).ToHashSet();
        }

        var playerIds = playerWithMinDamage;

        if (playerWithMinDamage.Count == 0)
        {
            playerIds = eventArgs.Context.PlayerIds.ToHashSet();
        }

        var scriptExecutionTask = context.Prefab.Events.OnDestruction.ExecuteAsync(
            new ScriptContext(
                eventArgs.Context.Provider,
                eventArgs.Context.FactionId,
                playerIds,
                eventArgs.Context.Sector,
                eventArgs.Context.TerritoryId
            )
            {
                ConstructId = eventArgs.ConstructId
            }
        );

        taskList.Add(scriptExecutionTask);

        await Task.WhenAll(taskList);

        context.PublishedEvents.TryAdd(nameof(NotifyConstructDestroyedAsync), true);
    }
}