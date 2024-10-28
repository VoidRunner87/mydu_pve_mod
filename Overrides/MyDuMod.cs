using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Overrides;
using Mod.DynamicEncounters.Overrides.Actions;
using Mod.DynamicEncounters.Overrides.Actions.Data;
using Mod.DynamicEncounters.Overrides.Actions.Party;
using Mod.DynamicEncounters.Overrides.ApiClient.Data;
using Mod.DynamicEncounters.Overrides.ApiClient.Services;
using Mod.DynamicEncounters.Overrides.Common;
using Mod.DynamicEncounters.Overrides.Common.Interfaces;
using Mod.DynamicEncounters.Overrides.Common.Services;
using Mod.DynamicEncounters.Overrides.Overrides.WeaponGrain;
using Newtonsoft.Json;
using NQ;
using NQ.Grains.Core;
using NQ.Interfaces;
using Orleans;
using Notifications = Mod.DynamicEncounters.Overrides.Notifications;

// ReSharper disable once CheckNamespace
public class MyDuMod : IMod
{
    private IServiceProvider _provider;
    private ILogger _logger;
    private WeaponGrainOverrides _weaponGrainOverrides;
    private readonly PlayerRateLimiter _playerRateLimiter = new(8);
    private IMyDuInjectionService _injection;

    public string GetName()
    {
        return "Mod.DynamicEncounters";
    }

    public Task Initialize(IServiceProvider provider)
    {
        _provider = provider;
        ModServiceProvider.Initialize(_provider);

        _logger = provider.GetRequiredService<ILogger<MyDuMod>>();
        _injection = new MyDuInjectionService();
        _weaponGrainOverrides = new WeaponGrainOverrides(_provider);

        var hookCallManager = provider.GetRequiredService<IHookCallManager>();
        hookCallManager.Register(
            "WeaponGrain.WeaponFireOnce",
            HookMode.Replace,
            _weaponGrainOverrides,
            nameof(WeaponGrainOverrides.WeaponFireOnce)
        );

        hookCallManager.Register(
            "PlayerGrain.InventoryReady",
            HookMode.Replace,
            this,
            nameof(InventoryReady)
        );

        return Task.CompletedTask;
    }

    public async Task InventoryReady(IIncomingGrainCallContext context)
    {
        var grain = context.Grain.AsReference<IPlayerGrain>();
        var playerId = (ulong)grain.GetPrimaryKeyLong();
        
        // TODO implement any initializing required

        await context.Invoke();
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
                    id = (ulong)ActionType.LoadPlayerBoardApp,
                    context = ModActionContext.Global,
                    label = "Actions\\Open player board"
                },
                new ModActionDefinition
                {
                    id = (ulong)ActionType.Interact,
                    context = ModActionContext.Element,
                    label = "Interact"
                },
                new ModActionDefinition
                {
                    id = (ulong)ActionType.LoadPlayerParty,
                    context = ModActionContext.Global,
                    label = "Group\\Open Group Widget"
                },
                new ModActionDefinition
                {
                    id = (ulong)ActionType.InviteToParty,
                    context = ModActionContext.Avatar,
                    label = "Group\\Invite to Group"
                }
            ]
        };

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
            _logger.LogError(e, "Failed to Trigger Mod Action {Id}", action.actionId);
            throw;
        }
    }

    private async Task TriggerActionInternal(ulong playerId, ModAction action)
    {
        _playerRateLimiter.TrackRequest(playerId);
        if (_playerRateLimiter.ExceededRateLimit(playerId))
        {
            _logger.LogWarning("Player {Player} Rate Limited", playerId);
            return;
        }
        
        _logger.LogInformation(
            "Received Trigger Action from Player({PlayerId} != {ActionPlayerId}): {ActionId} | {Content}",
            playerId,
            action.playerId,
            action.actionId,
            action.payload
        );

        var questApi = new PveModQuestsApiClient(_provider);
        var partyApi = new PveModPartyApiClient(_provider);

        switch ((ActionType)action.actionId)
        {
            case ActionType.CreateParty:
                await partyApi.CreateParty(playerId, CancellationToken.None)
                    .ContinueWith(x => x.Result.NotifyPlayer(_provider, playerId));
                break;
            case ActionType.SetPlayerLocation:
                // TODO
                break;
            case ActionType.LeaveParty:
                await partyApi.LeaveParty(playerId, CancellationToken.None)
                    .ContinueWith(x => x.Result.NotifyPlayer(_provider, playerId));
                break;
            case ActionType.DisbandParty:
                await partyApi.DisbandParty(playerId, CancellationToken.None)
                    .ContinueWith(x => x.Result.NotifyPlayer(_provider, playerId));
                break;
            case ActionType.CancelPartyInvite:
                await partyApi.CancelInvite(playerId, action.PayloadAs<PartyRequest>().PlayerId, CancellationToken.None)
                    .ContinueWith(x => x.Result.NotifyPlayer(_provider, playerId));
                break;
            case ActionType.AcceptPartyRequest:
                await partyApi.AcceptRequest(
                    playerId, 
                    action.PayloadAs<PartyRequest>().PlayerId,
                    CancellationToken.None
                ).ContinueWith(x => x.Result.NotifyPlayer(_provider, playerId));
                break;
            case ActionType.RejectPartyRequest:
                await partyApi.RejectRequest(
                    playerId,
                    action.PayloadAs<PartyRequest>().PlayerId,
                    CancellationToken.None
                ).ContinueWith(x => x.Result.NotifyPlayer(_provider, playerId));
                break;
            case ActionType.SetPartyRole:
                var setPartyRoleRequest = action.PayloadAs<PartyRequest>();
                await partyApi.SetPartyRole(
                    playerId,
                    setPartyRoleRequest.Role,
                    CancellationToken.None
                ).ContinueWith(x => x.Result.NotifyPlayer(_provider, playerId));
                break;
            case ActionType.LoadPlayerParty:
                await _injection.InjectJs(playerId, Resources.CommonJs);
                
                var fetchPlayerPartyForLoad = new FetchPartyDataAction(_provider);
                await fetchPlayerPartyForLoad.HandleAction(playerId, action);
                
                var renderPlayerParty = new RenderPartyAppAction();
                await renderPlayerParty.HandleAction(playerId, action);
                break;
            case ActionType.FetchPlayerParty:
                var fetchPlayerParty = new FetchPartyDataAction(_provider);
                await fetchPlayerParty.HandleAction(playerId, action);
                break;
            case ActionType.Interact:
                var interactAction = new InteractAction(_provider);
                await interactAction.HandleAction(playerId, action);
                break;
            case ActionType.GiveTakePlayerItems:
                var giveItemsToPlayerAction = new GiveTakePlayerItemsAction(_provider);
                await giveItemsToPlayerAction.HandleAction(playerId, action);
                break;
            case ActionType.LoadBoardApp:
                await _injection.InjectJs(playerId, Resources.CommonJs);
                await _injection.InjectJs(playerId, Resources.CreateRootDivJs);
                await _injection.InjectJs(playerId, "window.modApi.setPage('npc');");

                await TriggerActionInternal(
                    playerId,
                    new ModAction
                    {
                        playerId = action.playerId,
                        actionId = (ulong)ActionType.RefreshNpcQuestList,
                        constructId = action.constructId,
                        elementId = action.elementId,
                        modName = action.modName,
                        payload = action.payload
                    }
                );

                await _injection.InjectCss(playerId, Resources.NpcAppCss);
                await _injection.InjectJs(playerId, Resources.NpcAppJs);
                break;
            case ActionType.LoadPlayerBoardApp:
                await _injection.InjectJs(playerId, Resources.CommonJs);
                await _injection.InjectJs(playerId, Resources.CreateRootDivJs);
                await _injection.InjectJs(playerId, "window.modApi.setPage('player');");

                await TriggerActionInternal(
                    playerId,
                    new ModAction
                    {
                        playerId = action.playerId,
                        actionId = (ulong)ActionType.RefreshPlayerQuestList,
                        constructId = action.constructId,
                        elementId = action.elementId,
                        modName = action.modName,
                        payload = action.payload
                    }
                );

                await _injection.InjectCss(playerId, Resources.NpcAppCss);
                await _injection.InjectJs(playerId, Resources.NpcAppJs);
                break;
            case ActionType.RefreshNpcQuestList:
                var refreshedNpcQuests = JsonConvert.DeserializeObject<QueryNpcQuests>(action.payload);

                var refreshedJsonData = await questApi.GetNpcQuests(
                    playerId,
                    refreshedNpcQuests.FactionId,
                    refreshedNpcQuests.TerritoryId,
                    refreshedNpcQuests.Seed
                );

                await _injection.UploadJson(playerId, "faction-quests", refreshedJsonData);
                await _injection.SetContext(playerId, new
                {
                    playerId,
                    factionId = refreshedNpcQuests.FactionId,
                    territoryId = refreshedNpcQuests.TerritoryId,
                    seed = refreshedNpcQuests.Seed
                });

                break;
            case ActionType.RefreshPlayerQuestList:

                var playerQuestJsonData = await questApi.GetPlayerQuestsAsync(
                    playerId
                );

                await _injection.UploadJson(playerId, "player-quests", playerQuestJsonData);
                await _injection.SetContext(playerId, new
                {
                    playerId
                });

                break;
            case ActionType.CloseBoard:
                await _injection.InjectJs(playerId, "modApi.removeAppRoot()");
                break;
            case ActionType.AcceptQuest:
                var acceptQuest = JsonConvert.DeserializeObject<AcceptQuest>(action.payload);

                var acceptQuestOutcome = await questApi.AcceptQuest(
                    acceptQuest.QuestId,
                    acceptQuest.PlayerId,
                    acceptQuest.FactionId,
                    acceptQuest.TerritoryId,
                    acceptQuest.Seed
                );

                _logger.LogInformation("Accept Quest Outcome {Outcome} {Success}",
                    acceptQuestOutcome?.Message,
                    acceptQuestOutcome?.Success
                );

                if (!acceptQuestOutcome.Success)
                {
                    await Notifications.ErrorNotification(_provider, playerId, $"Failed: {acceptQuestOutcome.Message}");
                    break;
                }

                await PlayMissionAcceptedSound(playerId);
                await Notifications.SimpleNotificationToPlayer(_provider, playerId, "Mission accepted");
                break;
            case ActionType.AbandonQuest:
                var abandonQuest = JsonConvert.DeserializeObject<AbandonQuest>(action.payload);

                var abandonQuestOutcome = await questApi.AbandonQuest(abandonQuest.QuestId, abandonQuest.PlayerId);

                if (!abandonQuestOutcome.Success)
                {
                    await Notifications.ErrorNotification(_provider, playerId,
                        $"Failed: {abandonQuestOutcome.Message}");
                    break;
                }

                await Notifications.SimpleNotificationToPlayer(_provider, playerId, "Mission abandoned");

                break;
        }
    }

    private async Task PlayMissionAcceptedSound(ulong playerId)
    {
        await _injection.InjectJs(playerId, "soundManager.playSoundEvent(3079547240);");
    }
}