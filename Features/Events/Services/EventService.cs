using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Events.Data;
using Mod.DynamicEncounters.Features.Events.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ;

namespace Mod.DynamicEncounters.Features.Events.Services;

public class EventService(IServiceProvider provider) : IEventService
{
    private readonly IEventRepository _repository = provider.GetRequiredService<IEventRepository>();

    private readonly IEventTriggerRepository
        _triggerRepository = provider.GetRequiredService<IEventTriggerRepository>();

    private readonly IScriptService _scriptService = provider.GetRequiredService<IScriptService>();

    private readonly ILogger<EventService> _logger = provider.CreateLogger<EventService>();

    public async Task PublishAsync(IEvent @event)
    {
        try
        {
            var data = @event.GetData<EventData>();
            
            var sector = data.Sector;
            var constructId = data.ConstructId; 

            await _repository.AddAsync(@event);

            var sum = await _repository.GetSumAsync(@event.Name, @event.PlayerId);

            // TODO FIX - it should return zero if there aren't any tackers.. this code here is incorrect
            var triggers = (await _triggerRepository.FindPendingByEventNameAndPlayerIdAsync(@event.Name, @event.PlayerId))
                .Where(t => t.ShouldTrigger(sum));

            var playerIds = new HashSet<ulong>();
            if (@event.PlayerId.HasValue)
            {
                playerIds.Add(@event.PlayerId.Value);
            }

            var taskList = new List<Task>();

            foreach (var trigger in triggers)
            {
                var task = RunTriggerAsync(trigger, playerIds, sector, constructId);

                taskList.Add(task);
            }

            await Task.WhenAll(taskList);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to publish Event {Event}", @event.Name);
        }
    }

    private async Task RunTriggerAsync(EventTriggerItem triggerItem, HashSet<ulong> playerIds, Vec3 sector, ulong? constructId)
    {
        try
        {
            if (string.IsNullOrEmpty(triggerItem.OnTriggerScript))
            {
                _logger.LogWarning(
                    "No Script to run for {Trigger} on PlayerId({PlayerId})",
                    triggerItem.EventName,
                    string.Join(", ", playerIds)
                );
                
                return;
            }

            await _scriptService.ExecuteScriptAsync(
                triggerItem.OnTriggerScript,
                new ScriptContext(provider, playerIds, sector)
                {
                    ConstructId = constructId
                }
            );

            foreach (var playerId in playerIds)
            {
                await _triggerRepository.AddTriggerTrackingAsync(playerId, triggerItem.Id);
            }
            
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Failed to Run Trigger {Trigger} on PlayerId({PlayerId})",
                triggerItem.EventName,
                string.Join(", ", playerIds)
            );
        }
    }
}