using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Threads.Handles;

public class ConstructBehaviorLoop : HighTickModLoop
{
    private readonly BehaviorTaskCategory _category;
    private readonly IServiceProvider _provider;
    private readonly ILogger<ConstructBehaviorLoop> _logger;
    private readonly IConstructBehaviorFactory _behaviorFactory;
    private readonly IConstructDefinitionFactory _constructDefinitionFactory;

    public static bool FeatureEnabled;
    public static readonly ConcurrentDictionary<ulong, ConstructHandleItem> ConstructHandles = [];
    public static readonly ConcurrentDictionary<ulong, DateTime> ConstructHandleHeartbeat = [];
    public static readonly object ListLock = new();

    public ConstructBehaviorLoop(
        ThreadId threadId,
        IThreadManager threadManager,
        CancellationToken token,
        int framesPerSecond,
        BehaviorTaskCategory category
    ) : base(framesPerSecond, threadId, threadManager, token)
    {
        _category = category;
        _provider = ModBase.ServiceProvider;
        _logger = _provider.CreateLogger<ConstructBehaviorLoop>();

        _behaviorFactory = _provider.GetRequiredService<IConstructBehaviorFactory>();
        _constructDefinitionFactory = _provider.GetRequiredService<IConstructDefinitionFactory>();
    }

    public override async Task Tick(TimeSpan deltaTime)
    {
        if (!FeatureEnabled)
        {
            return;
        }

        var taskList = new List<Task>();

        var sw = new Stopwatch();
        sw.Start();

        lock (ListLock)
        {
            if (ConstructHandles.Count == 0)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(500));
                ReportHeartbeat();
                return;
            }
            
            foreach (var kvp in ConstructHandles)
            {
                var task = Task.Run(() => RunIsolatedAsync(() => TickConstructHandle(deltaTime, kvp.Value)));
                taskList.Add(task);
            }
        }

        await Task.WhenAll(taskList);

        StatsRecorder.Record(_category, sw.ElapsedMilliseconds);
        ReportHeartbeat();
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

        var finalBehaviors = new List<IConstructBehavior>();

        var behaviors = _behaviorFactory.CreateBehaviors(
            handleItem.ConstructId,
            constructDef,
            handleItem.JsonProperties.Behaviors
        ).Where(x => x.Category == _category);

        finalBehaviors.AddRange(behaviors);

        // TODO TerritoryId
        var context = await ConstructBehaviorContextCache.Data
            .TryGetOrSetValue(
                handleItem.ConstructId,
                () => Task.FromResult(
                    new BehaviorContext(
                        handleItem.ConstructId,
                        handleItem.FactionId,
                        null,
                        handleItem.Sector,
                        _provider,
                        constructDef
                    )
                    {
                        Properties = new ConcurrentDictionary<string, object>(handleItem.JsonProperties.Context)
                    }
                )
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

        ReportHeartbeat();
    }

    public static void RecordConstructHeartBeat(ulong constructId)
    {
        ConstructHandleHeartbeat.AddOrUpdate(
            constructId,
            _ => DateTime.UtcNow,
            (_, _) => DateTime.UtcNow
        );
    }
}