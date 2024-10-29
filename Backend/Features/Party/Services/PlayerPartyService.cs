using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Common;
using Mod.DynamicEncounters.Features.Common.Services;
using Mod.DynamicEncounters.Features.NQ.Interfaces;
using Mod.DynamicEncounters.Features.Party.Data;
using Mod.DynamicEncounters.Features.Party.Interfaces;
using NQ;

namespace Mod.DynamicEncounters.Features.Party.Services;

public class PlayerPartyService(IServiceProvider provider) : IPlayerPartyService
{
    private readonly IPlayerPartyRepository _repository = provider.GetRequiredService<IPlayerPartyRepository>();

    public async Task<PartyOperationOutcome> CreateParty(PlayerId instigatorPlayerId)
    {
        if (await _repository.IsInAParty(instigatorPlayerId))
        {
            return PartyOperationOutcome.AlreadyInAParty();
        }

        var groupId = await _repository.CreateParty(instigatorPlayerId);

        return PartyOperationOutcome.Successful(groupId, "Group created");
    }

    public async Task<PartyOperationOutcome> RequestJoinParty(PlayerId instigatorPlayerId, PlayerId targetPlayerId)
    {
        if (await _repository.IsInAParty(instigatorPlayerId))
        {
            return PartyOperationOutcome.AlreadyInAParty();
        }
        
        if (!await _repository.IsInAParty(targetPlayerId))
        {
            return PartyOperationOutcome.PlayerNotInAParty();
        }

        var groupId = await _repository.FindPartyGroupId(targetPlayerId);
        await _repository.AddPendingPartyRequest(groupId, instigatorPlayerId);

        return PartyOperationOutcome.Successful(groupId, "Request sent");
    }

    public async Task<PartyOperationOutcome> RequestJoinParty(PlayerId instigatorPlayerId, string playerName)
    {
        var playerService = provider.GetRequiredService<IPlayerService>();
        var playerId = await playerService.FindPlayerIdByName(playerName);

        if (playerId == null)
        {
            return PartyOperationOutcome.PlayerNotFound(playerName);
        }

        return await RequestJoinParty(instigatorPlayerId, playerId.Value);
    }

    public async Task<PartyOperationOutcome> DisbandParty(PlayerId instigatorPlayerId)
    {
        if (!await _repository.IsInAParty(instigatorPlayerId))
        {
            return PartyOperationOutcome.PlayerNotInAParty();
        }

        var groupId = await _repository.FindPartyGroupId(instigatorPlayerId);

        if (!await _repository.IsPartyLeader(groupId, instigatorPlayerId))
        {
            return PartyOperationOutcome.MustBePartyLeaderToDisband();
        }

        await _repository.DisbandParty(groupId);

        return PartyOperationOutcome.Disbanded(groupId);
    }

    public async Task<PartyOperationOutcome> LeaveParty(PlayerId playerId)
    {
        if (!await _repository.IsInAParty(playerId))
        {
            return PartyOperationOutcome.PlayerNotInAParty();
        }
        
        var groupId = await _repository.FindPartyGroupId(playerId);
        
        await _repository.RemovePlayerFromPartyAndFindNewLeader(groupId, playerId);
        
        return PartyOperationOutcome.Successful(groupId, "Left the group");
    }

    public async Task<PartyOperationOutcome> PromoteToPartyLeader(PlayerId instigatorPlayerId, PlayerId newLeaderPlayerId)
    {
        if (!await _repository.IsInAParty(instigatorPlayerId))
        {
            return PartyOperationOutcome.PlayerNotInAParty();
        }
        
        if (!await _repository.IsInAParty(newLeaderPlayerId))
        {
            return PartyOperationOutcome.PlayerNotInAParty();
        }

        if (!await _repository.IsAcceptedMember(newLeaderPlayerId))
        {
            return PartyOperationOutcome.NotAnAcceptedMember();
        }
        
        var leaderGroupId = await _repository.FindPartyGroupId(instigatorPlayerId);
        var memberGroupId = await _repository.FindPartyGroupId(newLeaderPlayerId);

        if (!leaderGroupId.Equals(memberGroupId))
        {
            return PartyOperationOutcome.PlayerOnDifferentParties();
        }

        if (!await _repository.IsPartyLeader(leaderGroupId, instigatorPlayerId))
        {
            return PartyOperationOutcome.MustBePartyLeaderPromoteAnotherPlayer();
        }

        await _repository.SetPartyLeader(leaderGroupId, newLeaderPlayerId);
        
        return PartyOperationOutcome.Successful(leaderGroupId, "Promoted to leader");
    }

    public async Task<PartyOperationOutcome> PromoteToPartyLeader(PlayerId instigatorPlayerId, string playerName)
    {
        var playerService = provider.GetRequiredService<IPlayerService>();
        var playerId = await playerService.FindPlayerIdByName(playerName);

        if (playerId == null)
        {
            return PartyOperationOutcome.PlayerNotFound(playerName);
        }

        return await PromoteToPartyLeader(instigatorPlayerId, playerId.Value);
    }

    public async Task<PartyOperationOutcome> InviteToParty(PlayerId instigatorPlayerId, PlayerId invitedPlayerId)
    {
        if (await _repository.IsInAParty(invitedPlayerId))
        {
            return PartyOperationOutcome.AlreadyInAParty();
        }
        
        if (!await _repository.IsInAParty(instigatorPlayerId))
        {
            var outcome = await CreateParty(instigatorPlayerId);
            if (!outcome.Success)
            {
                return outcome;
            }
        }

        var groupId = await _repository.FindPartyGroupId(instigatorPlayerId);
        await _repository.AddPendingPartyInvite(groupId, invitedPlayerId);

        var playerService = provider.GetRequiredService<IPlayerService>();
        var instigatorName = await playerService.FindPlayerNameById(instigatorPlayerId);
        
        var playerAlertService = provider.GetRequiredService<IPlayerAlertService>();
        await playerAlertService.SendInfoAlert(invitedPlayerId, $"{instigatorName} invited you to a group");

        return PartyOperationOutcome.Successful(groupId, "Invite sent");
    }

    public async Task<PartyOperationOutcome> InviteToParty(PlayerId instigatorPlayerId, string playerName)
    {
        var playerService = provider.GetRequiredService<IPlayerService>();
        var playerId = await playerService.FindPlayerIdByName(playerName);

        if (playerId == null)
        {
            return PartyOperationOutcome.PlayerNotFound(playerName);
        }

        return await InviteToParty(instigatorPlayerId, playerId.Value);
    }

    public async Task<PartyOperationOutcome> AcceptPartyInvite(PlayerId invitedPlayerId)
    {
        if (!await _repository.IsInAParty(invitedPlayerId))
        {
            return PartyOperationOutcome.PlayerNotInAParty();
        }

        var groupId = await _repository.FindPartyGroupId(invitedPlayerId);
        
        if (await _repository.IsAcceptedMember(invitedPlayerId))
        {
            return PartyOperationOutcome.AlreadyAccepted(groupId);
        }
        
        await _repository.AcceptPendingInvite(invitedPlayerId);
        
        return PartyOperationOutcome.Successful(groupId, "Accepted");
    }

    public async Task<PartyOperationOutcome> CancelPartyInviteRequest(PlayerId instigatorPlayerId, PlayerId invitedPlayerId)
    {
        if (!await _repository.IsInAParty(invitedPlayerId))
        {
            return PartyOperationOutcome.PlayerNotInAParty();
        }

        var groupId = await _repository.FindPartyGroupId(instigatorPlayerId);
        
        if (!await _repository.IsPartyLeader(groupId, instigatorPlayerId))
        {
            return PartyOperationOutcome.MustBePartyLeader();
        }

        if (await _repository.IsAcceptedMember(invitedPlayerId))
        {
            return PartyOperationOutcome.AlreadyAccepted(groupId);
        }
        
        await _repository.RemoveNonLeaderPlayerFromParty(groupId, invitedPlayerId);
        
        return PartyOperationOutcome.Successful(groupId, "Canceled");
    }

    public async Task<PartyOperationOutcome> CancelPartyInviteRequest(PlayerId instigatorPlayerId, string playerName)
    {
        var playerService = provider.GetRequiredService<IPlayerService>();
        var playerId = await playerService.FindPlayerIdByName(playerName);

        if (playerId == null)
        {
            return PartyOperationOutcome.PlayerNotFound(playerName);
        }

        return await CancelPartyInviteRequest(instigatorPlayerId, playerId.Value);
    }

    public async Task<PartyOperationOutcome> AcceptPartyRequest(PlayerId instigatorPlayerId, PlayerId invitedPlayerId)
    {
        if (!await _repository.IsInAParty(invitedPlayerId))
        {
            return PartyOperationOutcome.PlayerNotInAParty();
        }

        var groupId = await _repository.FindPartyGroupId(instigatorPlayerId);
        
        if (!await _repository.IsPartyLeader(groupId, instigatorPlayerId))
        {
            return PartyOperationOutcome.MustBePartyLeader();
        }

        if (await _repository.IsAcceptedMember(invitedPlayerId))
        {
            return PartyOperationOutcome.AlreadyAccepted(groupId);
        }
        
        await _repository.AcceptPartyRequest(invitedPlayerId);
        
        return PartyOperationOutcome.Successful(groupId, "Accepted");
    }

    public async Task<PartyOperationOutcome> AcceptPartyRequest(PlayerId instigatorPlayerId, string playerName)
    {
        var playerService = provider.GetRequiredService<IPlayerService>();
        var playerId = await playerService.FindPlayerIdByName(playerName);
        
        if (playerId == null)
        {
            return PartyOperationOutcome.PlayerNotFound(playerName);
        }

        return await AcceptPartyRequest(instigatorPlayerId, playerId.Value);
    }

    public async Task<PartyOperationOutcome> KickPartyMember(PlayerId instigatorPlayerId, PlayerId playerToKick)
    {
        if (!await _repository.IsInAParty(instigatorPlayerId))
        {
            return PartyOperationOutcome.PlayerNotInAParty();
        }
        
        var leaderGroupId = await _repository.FindPartyGroupId(instigatorPlayerId);
        
        if (!await _repository.IsPartyLeader(leaderGroupId, instigatorPlayerId))
        {
            return PartyOperationOutcome.MustBePartyLeader();
        }
        
        if (!await _repository.IsInAParty(playerToKick))
        {
            return PartyOperationOutcome.PlayerNotInAParty();
        }
        
        var memberGroupId = await _repository.FindPartyGroupId(playerToKick);

        if (!leaderGroupId.Equals(memberGroupId))
        {
            return PartyOperationOutcome.PlayerOnDifferentParties();
        }

        await _repository.RemoveNonLeaderPlayerFromParty(leaderGroupId, playerToKick);
        
        return PartyOperationOutcome.Successful(leaderGroupId, "Player removed");
    }

    public async Task<PartyOperationOutcome> KickPartyMember(PlayerId instigatorPlayerId, string playerName)
    {
        var playerService = provider.GetRequiredService<IPlayerService>();
        var playerId = await playerService.FindPlayerIdByName(playerName);
        
        if (playerId == null)
        {
            return PartyOperationOutcome.PlayerNotFound(playerName);
        }

        return await KickPartyMember(instigatorPlayerId, playerId.Value);
    }

    public async Task<PartyOperationOutcome> SetPlayerPartyRole(PlayerId instigatorPlayerId, string role)
    {
        if (!await _repository.IsInAParty(instigatorPlayerId))
        {
            return PartyOperationOutcome.PlayerNotInAParty();
        }

        // basic sanitation
        role = role.ToLower().Trim().Truncate(10);
        
        var groupId = await _repository.FindPartyGroupId(instigatorPlayerId);

        await _repository.SetPlayerPartyRole(instigatorPlayerId, role);
        
        return PartyOperationOutcome.Successful(groupId, $"Role '{role}' set");
    }

    public Task<IEnumerable<PlayerPartyItem>> GetPartyByPlayerId(PlayerId playerId)
    {
        return _repository.GetPartyByPlayerId(playerId);
    }
}