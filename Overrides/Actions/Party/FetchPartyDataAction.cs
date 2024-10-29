using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Overrides.Actions.Data;
using Mod.DynamicEncounters.Overrides.ApiClient.Data;
using Mod.DynamicEncounters.Overrides.ApiClient.Interfaces;
using Mod.DynamicEncounters.Overrides.ApiClient.Services;
using Mod.DynamicEncounters.Overrides.Common.Interfaces;
using Mod.DynamicEncounters.Overrides.Common.Services;
using Newtonsoft.Json;
using NQ;

namespace Mod.DynamicEncounters.Overrides.Actions.Party;

public class FetchPartyDataAction(IServiceProvider provider) : IModActionHandler
{
    private readonly IConstructService _constructService = new ConstructService(provider);
    private readonly IPlayerService _playerService = new PlayerService(provider);
    private readonly IPveModPartyApiClient _partyApiClient = new PveModPartyApiClient(provider);
    private readonly ILogger<FetchPartyDataAction> _logger = provider.GetRequiredService<ILoggerFactory>()
        .CreateLogger<FetchPartyDataAction>();

    public async Task HandleAction(ulong playerId, ModAction action)
    {
        var injection = ModServiceProvider.Get<IMyDuInjectionService>();

        var sw = new Stopwatch();
        sw.Start();

        var list = (await _partyApiClient.GetPartyByPlayerId(playerId, CancellationToken.None)).ToList();

        var leader = list.FirstOrDefault(x => x.IsLeader);

        if (leader == null)
        {
            _logger.LogInformation("Party Data: {Json}", JsonConvert.SerializeObject(new PartyData()));
            await injection.UploadJson(playerId, "player-party", new PartyData());
            return;
        }

        var pendingInvite = list.Where(x => x.IsPendingAcceptInvite).ToList();
        var pendingRequest = list.Where(x => x.IsPendingAcceptRequest).ToList();
        var members = list
            .Where(x => !x.IsPendingAcceptRequest && !x.IsPendingAcceptInvite && !x.IsLeader);

        var pendingList = pendingInvite.Select(x => x.PlayerId).ToList();
        pendingList.AddRange(pendingRequest.Select(x => x.PlayerId).ToList());
        var pendingSet = pendingList.ToHashSet();

        var pendingInviteList = pendingInvite.Select(x => MapToModelPending(x, playerId));
        var pendingRequestList = pendingRequest.Select(x => MapToModelPending(x, playerId));
        
        if (pendingSet.Contains(playerId))
        {
            var pendingPartyData = new PartyData
            {
                GroupId = list.First().GroupId,
                PlayerIsLeader = false,
                PlayerIsPending = true,
                Leader = MapToModelRestricted(leader, playerId),
                Members = [],
                PendingAccept = pendingRequestList.Where(x => x.PlayerId == playerId),
                Invited = pendingInviteList.Where(x => x.PlayerId == playerId)
            };
            
            await injection.UploadJson(playerId, "player-party", pendingPartyData);
            _logger.LogInformation("Party Data: {Json}", JsonConvert.SerializeObject(pendingPartyData));
            return;
        }

        var membersMappedTask = await Task.WhenAll(members.Select(x => MapToModelMember(x, playerId)));

        var partyData = new PartyData
        {
            GroupId = list.First().GroupId,
            PlayerIsLeader = playerId == leader.PlayerId,
            PlayerIsPending = pendingSet.Contains(playerId),
            Leader = await MapToModelMember(leader, playerId),
            Invited = pendingInviteList,
            PendingAccept = pendingRequestList,
            Members = membersMappedTask
        };

        _logger.LogInformation("Party Data: {Json}", JsonConvert.SerializeObject(partyData));
        // _logger.LogInformation("FetchPartyDataAction Took: {Time}ms", sw.ElapsedMilliseconds);

        await injection.UploadJson(playerId, "player-party", partyData);
    }
    
    private PartyData.PartyMemberEntry MapToModelRestricted(PlayerPartyItem item, ulong selfPlayerId)
    {
        var entry = new PartyData.PartyMemberEntry
        {
            PlayerId = item.PlayerId,
            PlayerName = item.PlayerName,
            IsSelf = item.PlayerId == selfPlayerId,
            IsConnected = false,
            IsLeader = item.IsLeader,
            Theme = item.Properties.Theme,
            Construct = new PartyData.PartyMemberEntry.ConstructData
            {
                ConstructId = 0,
                Size = 0,
                ConstructKind = ConstructKind.UNIVERSE,
                ConstructName = "",
                ShieldRatio = 0,
                CoreStressRatio = 0
            }
        };

        return entry;
    }

    private async Task<PartyData.PartyMemberEntry> MapToModelMember(PlayerPartyItem item, ulong selfPlayerId)
    {
        var entry = new PartyData.PartyMemberEntry
        {
            PlayerId = item.PlayerId,
            PlayerName = item.PlayerName,
            IsSelf = item.PlayerId == selfPlayerId,
            IsConnected = item.IsConnected,
            IsLeader = item.IsLeader,
            Role = item.Properties.Role,
            Theme = item.Properties.Theme,
            Construct = new PartyData.PartyMemberEntry.ConstructData
            {
                ConstructId = 0,
                Size = 0,
                ConstructKind = ConstructKind.UNIVERSE,
                ConstructName = "",
                ShieldRatio = 0,
                CoreStressRatio = 0
            }
        };

        var playerPosition = await _playerService.GetPlayerPositionCached(item.PlayerId);
        if (playerPosition is not { Valid: true, ConstructId: > 0 }) return entry;

        var constructItem = await _constructService.GetConstructInfoCached(playerPosition.ConstructId);

        if (constructItem == null) return entry;

        entry.Construct = new PartyData.PartyMemberEntry.ConstructData
        {
            ConstructId = constructItem.Id,
            ConstructKind = constructItem.Kind,
            Size = constructItem.Size,
            ConstructName = constructItem.Name,
            ShieldRatio = constructItem.ShieldRatio
        };

        switch (constructItem.Kind)
        {
            case ConstructKind.STATIC:
                entry.Construct.Size = constructItem.Size;
                break;
            case ConstructKind.SPACE:
            case ConstructKind.DYNAMIC:
                entry.Construct.Size = constructItem.Size;
                entry.Construct.CoreStressRatio = 1 - await _constructService
                    .GetCoreStressRatioCached(playerPosition.ConstructId);
                break;
        }

        return entry;
    }

    private PartyData.PartyMemberEntry MapToModelPending(PlayerPartyItem? item, ulong selfPlayerId)
    {
        if (item == null)
        {
            return null;
        }
        
        var entry = new PartyData.PartyMemberEntry
        {
            PlayerId = item.PlayerId,
            IsSelf = selfPlayerId == item.PlayerId,
            PlayerName = item.PlayerName,
            IsConnected = item.IsConnected,
            IsLeader = item.IsLeader,
            Role = item.Properties.Role,
            Theme = item.Properties.Theme,
            Construct = null
        };

        return entry;
    }
}