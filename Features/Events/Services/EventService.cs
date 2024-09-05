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
        var sector = new Vec3();
        ulong? constructId = null; 

        if (@event.Data != null)
        {
            var data = @event.DataAsJToken();
            var sectorVal = data.Value<Vec3?>("Sector");
            if (sectorVal.HasValue)
            {
                sector = sectorVal.Value;
            }

            constructId = data.Value<ulong?>("ConstructId");
        }

        await _repository.AddAsync(@event);

        var sum = await _repository.GetSumAsync(@event.Name, @event.PlayerId);

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

            await _triggerRepository.AddTriggerTrackingAsync(triggerItem.Id);
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