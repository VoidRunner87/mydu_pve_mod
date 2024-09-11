using System;
using System.Threading.Tasks;
using System.Timers;
using Backend.Storage;
using BotLib.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;
using NQutils.Def;

namespace Mod.DynamicEncounters;

public class DebugLoop : ModBase
{
    public override Task Start()
    {
        var provider = ServiceProvider;
        var featureService = provider.GetRequiredService<IFeatureReaderService>();
        
        var taskCompletionSource = new TaskCompletionSource();
        
        var timer = new Timer(3000);
        timer.Elapsed += async (sender, args) =>
        {
            if (await featureService.GetEnabledValue<DebugLoop>(false))
            {
                await OnTimer(sender, args);
            }
        };
        timer.Start();

        return taskCompletionSource.Task;
    }

    private async Task OnTimer(object? sender, ElapsedEventArgs args)
    {
        var provider = ServiceProvider;
        var featureService = provider.GetRequiredService<IFeatureReaderService>();
        var logger = provider.CreateLogger<DebugLoop>();
        
        var constructId = (ulong)await featureService.GetIntValueAsync("DEBUG_ConstructId", 0);

        var orleans = provider.GetOrleans();

        var constructElementsGrain = orleans.GetConstructElementsGrain(new ConstructId { constructId = constructId });

        var containers = await constructElementsGrain.GetElementsOfType<ContainerUnit>();

        try
        {
            var bank = provider.GetGameplayBank();
            var itemDef = bank.GetDefinition(4234772167);
            var ironOreItemInfo = itemDef.AsItemInfo();

            var itemStorageService = provider.GetRequiredService<IItemStorageService>();
            
            var transaction = await itemStorageService.MakeTransaction(
                Tag.ExternalReward(2)
            );
            
            foreach (var containerId in containers)
            {
                var containerGrain = orleans.GetContainerGrain(containerId);

                try
                {
                    await containerGrain.Add(
                        transaction,
                        new StorageSlot
                        {
                            content = ironOreItemInfo,
                            position = 10,
                            quantity = 1
                        },
                        4
                    );
                }
                catch (Exception e)
                {
                    logger.LogInformation(e, "Failed to add to container {Id}", containerId);
                }
            }
            
            await transaction.Commit();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        // var engines = await constructElementsGrain.GetElementsOfType<EngineUnit>();
        //
        // foreach (var engine in engines)
        // {
        //     var engineElement = await constructElementsGrain.GetElement(engine);
        //     
        //     logger.LogInformation("Engine {Id}", engine);
        //     foreach (var kvp in engineElement.properties)
        //     {
        //         logger.LogInformation("P > {Prop} = {Value}", kvp.Key, kvp.Value.value);
        //     }
        //     
        //     foreach (var kvp in engineElement.serverProperties)
        //     {
        //         logger.LogInformation("SP > {Prop} = {Value}", kvp.Key, kvp.Value.value);
        //     }
        //
        //     logger.LogInformation("----------------");
        // }
    }
}