using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters;

public class ConstructBehaviorLoop : HighTickModLoop
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<ConstructBehaviorLoop> _logger;
    private readonly IConstructInMemoryBehaviorContextRepository _inMemoryContextRepo;
    private readonly IConstructHandleRepository _constructHandleRepository;
    private readonly IConstructBehaviorFactory _behaviorFactory;
    private readonly IConstructDefinitionFactory _constructDefinitionFactory;
    private readonly IFeatureReaderService _featureService;

    private bool _featureEnabled;
    private ConcurrentDictionary<ulong, ConstructHandleItem> _constructHandles = [];

    private ConcurrentDictionary<ulong, BehaviorContext> _behaviorContexts = [];
    
    private static readonly object listLock = new();

    public ConstructBehaviorLoop(int framesPerSecond) : base(framesPerSecond)
    {
        _provider = ServiceProvider;
        _logger = _provider.CreateLogger<ConstructBehaviorLoop>();
        _inMemoryContextRepo = _provider.GetRequiredService<IConstructInMemoryBehaviorContextRepository>();

        _constructHandleRepository = _provider.GetRequiredService<IConstructHandleRepository>();
        _behaviorFactory = _provider.GetRequiredService<IConstructBehaviorFactory>();
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
            _featureEnabled = await _featureService.GetEnabledValue<ConstructBehaviorLoop>(false);
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

            lock (listLock)
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

        lock (listLock)
        {
            foreach (var kvp in _constructHandles)
            {
                var task = RunIsolatedAsync(() => TickConstructHandle(deltaTime, kvp.Value));
                taskList.Add(task);
            }
        }

        await Task.WhenAll(taskList);
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
        
        var constructDef = _constructDefinitionFactory.Create(handleItem.ConstructDefinitionItem);

        var finalBehaviors = new List<IConstructBehavior>
        {
            new AliveCheckBehavior(handleItem.ConstructId, constructDef)
                .WithErrorHandler(),
            new SelectTargetBehavior(handleItem.ConstructId, constructDef)
                .WithErrorHandler()
        };

        var behaviors = handleItem.ConstructDefinitionItem
            .InitialBehaviors.Select(b => _behaviorFactory.Create(handleItem.ConstructId, constructDef, b));

        finalBehaviors.AddRange(behaviors);
        finalBehaviors.Add(new UpdateLastControlledDateBehavior(handleItem.ConstructId).WithErrorHandler());

        if (!_behaviorContexts.TryGetValue(handleItem.ConstructId, out var context))
        {
            context = new BehaviorContext(handleItem.Sector, Bot, _provider, constructDef);
            _behaviorContexts.TryAdd(handleItem.ConstructId, context);
            
            _logger.LogInformation("NEW CONTEXT {Construct}", handleItem.ConstructId);
        }
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