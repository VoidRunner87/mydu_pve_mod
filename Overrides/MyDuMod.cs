using System;
using System.Threading.Tasks;
using Backend;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters;
using Mod.DynamicEncounters.Common;
using NQ;
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
        _logger.LogInformation("Received Trigger Action: {Id}", action.actionId);

        switch ((ModActionType)action.actionId)
        {
            case ModActionType.LoadNPCApp:
                await InjectJs(playerId, Resources.CommonJs);
                await Task.Delay(500);
                await InjectJs(playerId, Resources.CreateRootDivJs);
                await Task.Delay(500);
                await InjectCss(playerId, Resources.NpcAppCss);
                await Task.Delay(500);
                await InjectJs(playerId, Resources.NpcAppJs);
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