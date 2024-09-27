using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters;

public class ConstructTargetingBehaviorLoop : HighTickModLoop
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<ConstructMovementBehaviorLoop> _logger;
    private readonly IConstructHandleRepository _constructHandleRepository;
    private readonly IConstructDefinitionFactory _constructDefinitionFactory;
    private readonly IFeatureReaderService _featureService;

    private bool _featureEnabled;
    private readonly ConcurrentDictionary<ulong, ConstructHandleItem> _constructHandles = [];
    
    private static readonly object ListLock = new();

    public ConstructTargetingBehaviorLoop(int framesPerSecond) : base(framesPerSecond)
    {
        _provider = ServiceProvider;
        _logger = _provider.CreateLogger<ConstructMovementBehaviorLoop>();

        _constructHandleRepository = _provider.GetRequiredService<IConstructHandleRepository>();
        _constructDefinitionFactory = _provider.GetRequiredService<IConstructDefinitionFactory>();

        _featureService = _provider.GetRequiredService<IFeatureReaderService>();
    }

    public override Task Start()
    {
        return Task.WhenAll(
            CheckFeatureEnabledTask(),
            UpdateConstructHandleListTask(),
            base.Start()
        );
    }

    private Task CheckFeatureEnabledTask()
    {
        var taskCompletionSource = new TaskCompletionSource();

        var timer = new Timer(10000);
        timer.Elapsed += async (_, _) =>
        {
            _featureEnabled = await _featureService.GetEnabledValue<ConstructMovementBehaviorLoop>(false);
        };
        timer.Start();

        return taskCompletionSource.Task;
    }

    private Task UpdateConstructHandleListTask()
    {
        var taskCompletionSource = new TaskCompletionSource();

        var timer = new Timer(2000);
        timer.Elapsed += async (_, _) =>
        {
            var items = await _constructHandleRepository.FindActiveHandlesAsync();

            lock (ListLock)
            {
                _constructHandles.Clear();
                foreach (var item in items)
                {
                    _constructHandles.TryAdd(item.ConstructId, item);
                }
            }
        };
        timer.Start();

        return taskCompletionSource.Task;
    }

    public override async Task Tick(TimeSpan deltaTime)
    {
        if (!_featureEnabled)
        {
            return;
        }

        var taskList = new List<Task>();

        var sw = new Stopwatch();
        sw.Start();
        
        lock (ListLock)
        {
            foreach (var kvp in _constructHandles)
            {
                if (kvp.Value.ConstructDefinitionItem == null) continue;
                if (!kvp.Value.ConstructDefinitionItem.InitialBehaviors.Contains("follow-target")) continue;
                
                var task = RunIsolatedAsync(() => TickConstructHandle(deltaTime, kvp.Value));
                taskList.Add(task);
            }
        }

        await Task.WhenAll(taskList);
        
        // _logger.LogInformation("Behavior Loop Count({Count}) Took: {Time}ms", taskList.Count, sw.ElapsedMilliseconds);
    }

    private async Task RunIsolatedAsync(Func<Task> taskFn)
    {
        try
        {
            await taskFn();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Construct Handle Task Failed");
        }
    }

    private async Task TickConstructHandle(TimeSpan deltaTime, ConstructHandleItem handleItem)
    {
        if (handleItem.ConstructDefinitionItem == null)
        {
            return;
        }

        if (handleItem.ConstructDefinitionItem.InitialBehaviors.Count == 0)
        {
            return;
        }

        if (handleItem.ConstructDefinitionItem.InitialBehaviors.Contains("wreck"))
        {
            return;
        }
        
        var constructDef = _constructDefinitionFactory.Create(handleItem.ConstructDefinitionItem);

        List<SelectTargetBehavior> finalBehaviors = [
            new SelectTargetBehavior(handleItem.ConstructId, constructDef)
        ];

        // TODO TerritoryId
        var context = await ConstructBehaviorContextCache.Data
            .TryGetValue(
                handleItem.ConstructId,
                () => Task.FromResult(new BehaviorContext(handleItem.FactionId, null, handleItem.Sector, Bot, _provider, constructDef))
            );
        
        context.DeltaTime = deltaTime.TotalSeconds;

        foreach (var behavior in finalBehaviors)
        {
            await behavior.InitializeAsync(context);
        }

        foreach (var behavior in finalBehaviors)
        {
            if (!context.IsBehaviorActive(behavior.GetType()))
            {
                continue;
            }

            await behavior.TickAsync(context);
        }
    }
}