using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Party.Data;
using Mod.DynamicEncounters.Features.Party.Interfaces;
using NQ;

namespace Mod.DynamicEncounters.Features.Party.Services;

public class PlayerPartyService(IServiceProvider provider) : IPlayerPartyService
{
    private readonly IPlayerPartyRepository _repository = provider.GetRequiredService<IPlayerPartyRepository>();

    public async Task<PartyOperationOutcome> CreateParty(PlayerId leaderPlayerId)
    {
        if (await _repository.IsInAParty(leaderPlayerId))
        {
            return PartyOperationOutcome.AlreadyInAParty();
        }

        var groupId = await _repository.CreateParty(leaderPlayerId);

        return PartyOperationOutcome.Successful(groupId, "Party created");
    }

    public async Task<PartyOperationOutcome> RequestJoinParty(PlayerId leaderPlayerId, PlayerId memberPlayerId)
    {
        if (!await _repository.IsInAParty(leaderPlayerId))
        {
            return PartyOperationOutcome.PlayerNotInAParty();
        }
        
        if (await _repository.IsInAParty(memberPlayerId))
        {
            return PartyOperationOutcome.AlreadyInAParty();
        }

        var groupId = await _repository.FindPartyGroupId(leaderPlayerId);
        await _repository.AddPendingPartyRequest(groupId, memberPlayerId);

        return PartyOperationOutcome.Successful(groupId, "Request sent");
    }

    public async Task<PartyOperationOutcome> DisbandParty(PlayerId leaderPlayerId)
    {
        if (!await _repository.IsInAParty(leaderPlayerId))
        {
            return PartyOperationOutcome.PlayerNotInAParty();
        }

        var groupId = await _repository.FindPartyGroupId(leaderPlayerId);

        if (!await _repository.IsPartyLeader(groupId, leaderPlayerId))
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
        
        return PartyOperationOutcome.Successful(groupId, "Left the party");
    }

    public async Task<PartyOperationOutcome> PromoteToPartyLeader(PlayerId leaderPlayerId, PlayerId newLeaderPlayerId)
    {
        if (!await _repository.IsInAParty(leaderPlayerId))
        {
            return PartyOperationOutcome.PlayerNotInAParty();
        }
        
        if (!await _repository.IsInAParty(newLeaderPlayerId))
        {
            return PartyOperationOutcome.PlayerNotInAParty();
        }
        
        var leaderGroupId = await _repository.FindPartyGroupId(leaderPlayerId);
        var memberGroupId = await _repository.FindPartyGroupId(newLeaderPlayerId);

        if (!leaderGroupId.Equals(memberGroupId))
        {
            return PartyOperationOutcome.PlayerOnDifferentParties();
        }

        if (!await _repository.IsPartyLeader(leaderGroupId, leaderPlayerId))
        {
            return PartyOperationOutcome.MustBePartyLeaderPromoteAnotherPlayer();
        }

        await _repository.SetPartyLeader(leaderGroupId, newLeaderPlayerId);
        
        return PartyOperationOutcome.Successful(leaderGroupId, "Promoted to leader");
    }

    public async Task<PartyOperationOutcome> InviteToParty(PlayerId leaderPlayerId, PlayerId invitedPlayerId)
    {
        if (await _repository.IsInAParty(invitedPlayerId))
        {
            return PartyOperationOutcome.AlreadyInAParty();
        }
        
        if (!await _repository.IsInAParty(leaderPlayerId))
        {
            var outcome = await CreateParty(leaderPlayerId);
            if (!outcome.Success)
            {
                return outcome;
            }
        }

        var groupId = await _repository.FindPartyGroupId(leaderPlayerId);
        await _repository.AddPendingPartyInvite(groupId, invitedPlayerId);

        return PartyOperationOutcome.Successful(groupId, "Invite sent");
    }

    public Task<IEnumerable<PlayerPartyItem>> GetPartyByPlayerId(PlayerId playerId)
    {
        return _repository.GetPartyByPlayerId(playerId);
    }
}