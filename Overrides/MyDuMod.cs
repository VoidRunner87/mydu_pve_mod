using System;
using System.Threading.Tasks;
using Backend;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Overrides;
using Mod.DynamicEncounters.Overrides.Actions.Data;
using Mod.DynamicEncounters.Overrides.ApiClient;
using Mod.DynamicEncounters.Overrides.Common;
using Mod.DynamicEncounters.Overrides.WeaponGrain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NQ;
using NQ.Grains.Core;
using NQutils;
using Orleans;
using Notifications = Mod.DynamicEncounters.Overrides.Notifications;

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
        // hookCallManager.Register(
        //     "WeaponGrain.WeaponFireOnce",
        //     HookMode.Replace,
        //     _weaponGrainOverrides,
        //     nameof(WeaponGrainOverrides.WeaponFireOnce)
        // );

        return Task.CompletedTask;
    }

    public Task<ModInfo> GetModInfoFor(ulong playerId, bool admin)
    {
        var res = new ModInfo
        {
            name = GetName(),
            actions =
            [
            ]
        };

        var loadNpcApp = new ModActionDefinition
        {
            id = (ulong)ActionType.LoadNpcApp,
            context = ModActionContext.Global,
            label = "Admin\\Load NPC APP"
        };

        res.actions.Add(loadNpcApp);

        return Task.FromResult(res);
    }

    public async Task TriggerAction(ulong playerId, ModAction action)
    {
        try
        {
            await TriggerActionInternal(playerId, action);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to Trigger Mod Action");
        }
    }

    private async Task TriggerActionInternal(ulong playerId, ModAction action)
    {
        _logger.LogInformation("Received Trigger Action: {Id} | {Content}", action.actionId, action.payload);

        var apiClient = new PveModQuestsApiClient(_provider);

        switch ((ActionType)action.actionId)
        {
            case ActionType.LoadNpcApp:
                await InjectJs(playerId, Resources.CommonJs);
                await InjectJs(playerId, Resources.CreateRootDivJs);

                await TriggerActionInternal(
                    playerId,
                    new ModAction
                    {
                        playerId = action.playerId,
                        actionId = (ulong)ActionType.RefreshNpcApp,
                        constructId = action.constructId,
                        elementId = action.elementId,
                        modName = action.modName,
                        payload = action.payload
                    }
                );

                await InjectCss(playerId, Resources.NpcAppCss);
                await InjectJs(playerId, Resources.NpcAppJs);
                break;
            case ActionType.RefreshNpcApp:
                var refreshedNpcQuests = JsonConvert.DeserializeObject<QueryNpcQuests>(action.payload);

                var refreshedJsonData = await apiClient.GetNpcQuests(
                    playerId,
                    refreshedNpcQuests.FactionId,
                    refreshedNpcQuests.TerritoryId,
                    refreshedNpcQuests.Seed
                );

                await UploadJson(playerId, "faction-quests", refreshedJsonData);
                await SetContext(playerId, new
                {
                    playerId,
                    factionId = refreshedNpcQuests.FactionId,
                    territoryId = refreshedNpcQuests.TerritoryId,
                    seed = refreshedNpcQuests.Seed
                });

                break;
            case ActionType.CloseNpcApp:
                await InjectJs(playerId, "modApi.removeAppRoot()");
                break;
            case ActionType.AcceptQuest:
                var acceptQuest = JsonConvert.DeserializeObject<AcceptQuest>(action.payload);

                var acceptQuestOutcome = await apiClient.AcceptQuest(
                    acceptQuest.QuestId,
                    acceptQuest.PlayerId,
                    acceptQuest.FactionId,
                    acceptQuest.TerritoryId,
                    acceptQuest.Seed
                );

                if (!acceptQuestOutcome.Success)
                {
                    await Notifications.ErrorNotification(_provider, playerId, $"Failed: {acceptQuestOutcome.Message}");
                    break;
                }
                
                await PlayMissionAcceptedSound(playerId);
                await Notifications.SimpleNotificationToPlayer(_provider, playerId, "Mission Accepted");
                break;
        }
    }

    private async Task PlayMissionAcceptedSound(ulong playerId)
    {
        await InjectJs(playerId, "soundManager.playSoundEvent(3079547240);");
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
    
    private Task SetContext(ulong playerId, object data)
    {
        var jsonString = JsonConvert.SerializeObject(data);
        
        return InjectJs(
            playerId,
            $"""
             modApi.setContext({jsonString});
             """
        );
    }

    private Task UploadJson(ulong playerId, string key, JToken data)
    {
        return InjectJs(
            playerId,
            $"""
             modApi.setResourceContents(`{key}`, `application/json`, `{data}`);
             """
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