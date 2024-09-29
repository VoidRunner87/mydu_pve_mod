using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Backend;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Overrides;
using Mod.DynamicEncounters.Overrides.Common;
using Newtonsoft.Json;
using NQ;
using NQ.Grains.Core;
using NQ.Interfaces;
using NQ.RDMS;
using NQutils;
using NQutils.Def;
using NQutils.Exceptions;
using Orleans;
using Orleans.CodeGeneration;
using ErrorCode = NQ.ErrorCode;

// ReSharper disable once CheckNamespace
public class MyDuMod : IMod
{
    private IServiceProvider _provider;
    private IClusterClient _orleans;
    private ILogger _logger;

    private Random _rnd = new();
    private IGameplayBank _bank;
    private IPub _pub;

    public string GetName()
    {
        return "Mod.DynamicEncounters";
    }

    public Task Initialize(IServiceProvider provider)
    {
        _provider = provider;
        _orleans = provider.GetRequiredService<IClusterClient>();
        _logger = provider.GetRequiredService<ILogger<MyDuMod>>();
        _bank = provider.GetRequiredService<IGameplayBank>();
        _pub = provider.GetRequiredService<IPub>();
        
        var hookCallManager = provider.GetRequiredService<IHookCallManager>();
        // hookCallManager.Register(
        //     "ConstructElementsGrain.ElementOperation",
        //     HookMode.Replace,
        //     this,
        //     nameof(ElementOperation)
        // );
        // hookCallManager.Register(
        //     "ConstructElementsGrain.UpdateElementProperty",
        //     HookMode.Replace,
        //     this,
        //     nameof(UpdateElementProperty)
        // );
        // hookCallManager.Register(
        //     "ConstructElementsGrain.UpdateElementProperties",
        //     HookMode.Replace,
        //     this,
        //     nameof(UpdateElementProperties)
        // );
        // hookCallManager.Register(
        //     "ConstructElementsGrain.BatchEdit",
        //     HookMode.Replace,
        //     this,
        //     nameof(BatchEdit)
        // );
        // hookCallManager.Register(
        //     "ConstructElementsGrain.GetElement",
        //     HookMode.Replace,
        //     this,
        //     nameof(GetElement)
        // );
        // hookCallManager.Register(
        //     "ContainerGrain.CreateLink",
        //     HookMode.Replace,
        //     this,
        //     nameof(CreateLink)
        // );
        // hookCallManager.Register(
        //     "PlayerGrain.GetWarpDestinations",
        //     HookMode.Replace,
        //     this,
        //     nameof(GetWarpDestinations)
        // );
        // hookCallManager.Register(
        //     "ConstructGrain.CanWarp",
        //     HookMode.Replace,
        //     this,
        //     nameof(CanWarp)
        // );
        
        return Task.CompletedTask;
    }

    public async Task CanWarp(IIncomingGrainCallContext ctx)
    {
        _logger.LogInformation("----------------------> CanWarp");
    }

    public async Task<OwnedConstructDataList> GetWarpDestinations(IIncomingGrainCallContext ctx)
    {
        await ctx.Invoke();
        var list = (OwnedConstructDataList)ctx.Result;

        var constructInfoGrain = _orleans.GetConstructInfoGrain(1002100);
        // _orleans.GetConstructElementsGrain(1002100)
        var constructInfo = await constructInfoGrain.Get();
        constructInfo.rData.position = new Vec3
        {
            x = 36952077.0929,
            y = 48864525.2406,
            z = 7020.3003
        };

        list.constructs.Add(
            constructInfo
        );
        
        return list;
    }

    public async Task CreateLink(IIncomingGrainCallContext ctx, ElementId containerId)
    {
        _logger.LogInformation("----------------------> CreateLink");
        
        await ctx.Invoke();
    }

    public async Task<ElementInfo> GetElement(IIncomingGrainCallContext ctx, ElementId elementId)
    {
        _logger.LogInformation("----------------------> BatchEdit");

        await ctx.Invoke();
        return (ElementInfo)ctx.Result;
    }

    public async Task BatchEdit(IIncomingGrainCallContext ctx, LinkBatchEdit batch)
    {
        _logger.LogInformation("----------------------> BatchEdit");
        
        // await _provider.GetRequiredService<IPub>().NotifyTopic(
        //     Topics.ConstructPlayers(batch.constructId),
        //     new NQutils.Messages.ModTriggerHudEventRequest(
        //         new NQ.ModTriggerHudEvent
        //         {
        //             eventName = "modinjectjs",
        //             eventPayload = $"CPPHud.addFailureNotification(\"NO NO NO\");",
        //         }
        //     )
        // );

        // throw new BusinessException(ErrorCode.Unauthorized);

        await ctx.Invoke();
    }

    public async Task<ElementPropertyUpdates> UpdateElementProperty(IIncomingGrainCallContext ctx, ElementPropertyUpdateInternal update)
    {
        _logger.LogInformation("----------------------> UpdateElementProperty");

        await ctx.Invoke();
        return (ElementPropertyUpdates) ctx.Result;
    }

    public async Task UpdateElementProperties(IIncomingGrainCallContext ctx, List<NQ.ElementPropertyUpdate> update, bool fromServer = true)
    {
        _logger.LogInformation("----------------------> UpdateElementProperties");

        await ctx.Invoke();
    }

    public async Task ElementOperation(IGrainMethodInvoker invoker, ElementOperation op)
    {
        _logger.LogInformation("----------------------> Element OP");
        
        await Task.Yield();
    }

    public Task<ModInfo> GetModInfoFor(ulong playerId, bool admin)
    {
        var res = new ModInfo
        {
            name = GetName(),
            actions =
            [
                new ModActionDefinition
                {
                    id = (ulong)ModActionType.LoadNPCApp,
                    context = ModActionContext.Global,
                    label = "Admin\\Load NPC APP"
                }
            ]
        };

        return Task.FromResult(res);
    }

    public async Task TriggerAction(ulong playerId, ModAction action)
    {
        _logger.LogInformation("Received Trigger Action: {Id} | {Content}", action.actionId, action.payload);

        switch ((ModActionType)action.actionId)
        {
            case ModActionType.LoadNPCApp:
                await UploadJson(playerId, "faction-quests", new object[]{
                    new { faction = 3, title = "Disrupt UEF Supplies", description = "" },
                    new { faction = 3, title = "Disrupt UEF Supplies", description = "" },
                    new { faction = 3, title = "Disrupt UEF Supplies", description = "" },
                    new { faction = 3, title = "Disrupt UEF Supplies", description = "" },
                    new { faction = 3, title = "Disrupt UEF Supplies", description = "" },
                });
                await InjectJs(playerId, Resources.CommonJs);
                await InjectJs(playerId, Resources.CreateRootDivJs);
                await InjectCss(playerId, Resources.NpcAppCss);
                await InjectJs(playerId, Resources.NpcAppJs);
                break;
            case ModActionType.CloseNPCApp:
                await InjectJs(playerId, "modApi.removeAppRoot()");
                break;
        }
    }

    private async Task InjectJs(ulong playerId, string code)
    {
        _logger.LogInformation("Inject JS {Length}", code.Length);
        
        await _pub.NotifyTopic(
            Topics.PlayerNotifications(playerId),
            new NQutils.Messages.ModTriggerHudEventRequest(
                new ModTriggerHudEvent
                {
                    eventName = "modinjectjs",
                    eventPayload = code
                }
            )
        );
    }
    
    private Task UploadJson(ulong playerId, string key, object data)
    {
        var jsonString = JsonConvert.SerializeObject(data);
        
        return InjectJs(
            playerId,
            $"""
            modApi.setResourceContents(`{key}`, `application/json`, `{jsonString}`);
            """
        );
    }
    
    private async Task InjectCss(ulong playerId, string code)
    {
        _logger.LogInformation("Inject CSS {Length}", code.Length);
        
        await _pub.NotifyTopic(
            Topics.PlayerNotifications(playerId),
            new NQutils.Messages.ModTriggerHudEventRequest(
                new ModTriggerHudEvent
                {
                    eventName = "modinjectjs",
                    eventPayload = $"""
                                   modApi.addInlineCss(`{code}`);
                                   """
                }
            )
        );
    }
}