using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Party.Data;
using NQ;

namespace Mod.DynamicEncounters.Features.Party.Interfaces;

public interface IPlayerPartyService
{
    Task<PartyOperationOutcome> CreateParty(PlayerId instigatorPlayerId);
    Task<PartyOperationOutcome> RequestJoinParty(PlayerId instigatorPlayerId, PlayerId targetPlayerId);
    Task<PartyOperationOutcome> DisbandParty(PlayerId instigatorPlayerId);
    Task<PartyOperationOutcome> LeaveParty(PlayerId playerId);
    Task<PartyOperationOutcome> PromoteToPartyLeader(PlayerId instigatorPlayerId, PlayerId newLeaderPlayerId);
    Task<PartyOperationOutcome> InviteToParty(PlayerId instigatorPlayerId, PlayerId invitedPlayerId);
    Task<PartyOperationOutcome> AcceptPartyInvite(PlayerId invitedPlayerId);
    Task<PartyOperationOutcome> CancelPartyInvite(PlayerId instigatorPlayerId, PlayerId invitedPlayerId);
    Task<PartyOperationOutcome> AcceptPartyRequest(PlayerId instigatorPlayerId, PlayerId invitedPlayerId);
    Task<PartyOperationOutcome> KickPartyMember(PlayerId instigatorPlayerId, PlayerId invitedPlayerId);
    Task<PartyOperationOutcome> SetPlayerPartyRole(PlayerId instigatorPlayerId, string role);
    Task<IEnumerable<PlayerPartyItem>> GetPartyByPlayerId(PlayerId playerId);
}