using System;
using System.Threading.Tasks;
using Backend;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Overrides;
using Mod.DynamicEncounters.Overrides.Common;
using Mod.DynamicEncounters.Overrides.WeaponGrain;
using Newtonsoft.Json;
using NQ;
using NQ.Grains.Core;
using NQutils;
using Orleans;

// ReSharper disable once CheckNamespace
public class MyDuMod : IMod
{
    private IServiceProvider _provider;
    private IClusterClient _orleans;
    private ILogger _logger;

    private Random _rnd = new();
    private IGameplayBank _bank;
    private IPub _pub;
    private WeaponGrainOverrides _weaponGrainOverrides;

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

        _weaponGrainOverrides = new WeaponGrainOverrides(_provider);
        
        var hookCallManager = provider.GetRequiredService<IHookCallManager>();
        hookCallManager.Register(
            "WeaponGrain.WeaponFireOnce",
            HookMode.Replace,
            _weaponGrainOverrides,
            nameof(WeaponGrainOverrides.WeaponFireOnce)
        );
        
        return Task.CompletedTask;
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