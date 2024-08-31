using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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

public class ConstructBehaviorLoop : ModBase
{
    public override async Task Loop()
    {
        var provider = ServiceProvider;
        var logger = provider.CreateLogger<ConstructBehaviorLoop>();

        var constructHandleRepository = provider.GetRequiredService<IConstructHandleRepository>();
        var behaviorFactory = provider.GetRequiredService<IConstructBehaviorFactory>();
        var constructDefinitionFactory = provider.GetRequiredService<IConstructDefinitionFactory>();

        var featureService = provider.GetRequiredService<IFeatureReaderService>();
        
        IEnumerable<ConstructHandleItem> constructHandleItems = new List<ConstructHandleItem>();
        double totalHandleItemsDeltaTime = 0;
        double totalDeltaTimeFeatureCheck = 0;
        var previousTime = DateTime.UtcNow;
        
        var isEnabled = await featureService.GetEnabledValue<ConstructBehaviorLoop>(false);
        
        try
        {
            while (true)
            {
                if (!isEnabled)
                {
                    await Task.Delay(10000);
                    isEnabled = await featureService.GetEnabledValue<ConstructBehaviorLoop>(false);
                    continue;
                }
                
                var sw = new Stopwatch();
                sw.Start();
                
                await Task.Delay(1/15 * 1000);

                var now = DateTime.UtcNow;
                var deltaTime = (now - previousTime).TotalSeconds;
                totalHandleItemsDeltaTime += deltaTime;
                totalDeltaTimeFeatureCheck += deltaTime;

                if (totalDeltaTimeFeatureCheck > 10)
                {
                    isEnabled = await featureService.GetEnabledValue<ConstructBehaviorLoop>(false);
                    if (!isEnabled)
                    {
                        continue;
                    }
                }
                
                if (totalHandleItemsDeltaTime > 2)
                {
                    constructHandleItems = (await constructHandleRepository.GetAllAsync()).ToList();
                    totalHandleItemsDeltaTime = 0;
                    logger.LogInformation("Fetched '{Count}' Construct Handles with Behavior", constructHandleItems.Count());
                }

                foreach (var handleItem in constructHandleItems)
                {
                    if (handleItem.ConstructDefinitionItem == null)
                    {
                        continue;
                    }
                    
                    var constructDef = constructDefinitionFactory.Create(handleItem.ConstructDefinitionItem);

                    var finalBehaviors = new List<IConstructBehavior>
                    {
                        new AliveCheckBehavior(handleItem.ConstructId, constructDef),
                        new SelectTargetBehavior(handleItem.ConstructId, constructDef)
                    };
                    
                    var behaviors = handleItem.ConstructDefinitionItem
                        .InitialBehaviors.Select(b => behaviorFactory.Create(handleItem.ConstructId, constructDef, b))
                        .ToList();
                    
                    finalBehaviors.AddRange(behaviors);
                    finalBehaviors.Add(new UpdateLastControlledDateBehavior(handleItem.ConstructId));

                    var context = new BehaviorContext(Bot, provider)
                    {
                        DeltaTime = deltaTime
                    };
                    
                    foreach (var behavior in finalBehaviors)
                    {
                        await behavior.InitializeAsync(context);
                    }

                    foreach (var behavior in finalBehaviors)
                    {
                        if (!behavior.IsActive())
                        {
                            continue;
                        }

                        await behavior.TickAsync(context);
                    }
                }
                
                // logger.LogInformation("Tick Time: {Time}ms", sw.ElapsedMilliseconds);
                previousTime = now;
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to execute {Name}", nameof(TaskQueueLoop));
            // TODO implement alerting on too many failures
        }
    }
}