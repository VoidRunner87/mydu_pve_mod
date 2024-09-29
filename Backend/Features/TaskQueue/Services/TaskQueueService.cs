using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Common;
using Mod.DynamicEncounters.Features.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.TaskQueue.Data;
using Mod.DynamicEncounters.Features.TaskQueue.Interfaces;
using Mod.DynamicEncounters.Helpers;
using Newtonsoft.Json.Linq;
using NQ;

namespace Mod.DynamicEncounters.Features.TaskQueue.Services;

public class TaskQueueService(IServiceProvider provider) : ITaskQueueService
{
    private const string ProcessQueueMessageCountFeatureName = "ProcessQueueMessageCount";

    private readonly ITaskQueueRepository _repository = provider.GetRequiredService<ITaskQueueRepository>();
    private readonly IFeatureReaderService _featureReaderService = provider.GetRequiredService<IFeatureReaderService>();
    private readonly ILogger<TaskQueueService> _logger = provider.CreateLogger<TaskQueueService>();
    
    public async Task ProcessQueueMessages()
    {
        var messageBatch = await _featureReaderService.GetIntValueAsync(ProcessQueueMessageCountFeatureName, 10);

        var messages = (await _repository.FindNextAsync(messageBatch)).ToList();
        
        _logger.LogDebug("{Count} messages to process", messages.Count);

        var taskList = new List<Task>();

        foreach (var message in messages)
        {
            switch (message.Command)
            {
                case "script":
                    var scriptActionFactory = provider.GetRequiredService<IScriptActionFactory>();
                    var scriptActionItem = JToken.FromObject(message.Data).ToObject<ScriptActionItem>();

                    var scriptAction = scriptActionFactory.Create(scriptActionItem);

                    var task = scriptAction.ExecuteAsync(
                        new ScriptContext(
                            provider,
                            scriptActionItem.FactionId,
                            new HashSet<ulong>(),
                            scriptActionItem.Sector ?? new Vec3(),
                            scriptActionItem.TerritoryId
                        )
                    ).OnError(exception =>
                    {
                        _logger.LogError(exception, "Failed to Dequeue Script Task");
                        foreach (var e in exception.InnerExceptions)
                        {
                            _logger.LogError(e, "Inner Exception");
                        }
                    });
                    
                    taskList.Add(task);
                    taskList.Add(_repository.TagCompleted(message.Id));
                    
                    break;
                default:
                    _logger.LogWarning("Command Type {Command} Not implemented. Message Ignored", message.Command);
                    taskList.Add(_repository.TagFailed(message.Id));
                    break;
            }
        }

        try
        {
            await Task.WhenAll(taskList);

            if (messages.Count > 0)
            {
                _logger.LogInformation("Processed {Count} messages", messages.Count);
            }
        }
        catch (AggregateException ae)
        {
            _logger.LogError(ae, "One or more tasks failed to execute");

            foreach (var exception in ae.InnerExceptions)
            {
                _logger.LogError(exception, "Failed to execute task");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to execute task queue processing");
        }
    }

    public Task EnqueueScript(ScriptActionItem script, DateTime? deliveryAt)
    {
        return _repository.AddAsync(
            new TaskQueueItem
            {
                Id = Guid.NewGuid(),
                Command = "script",
                DeliveryAt = deliveryAt ?? DateTime.UtcNow,
                Data = script,
                Status = "scheduled"
            }
        );
    }
}