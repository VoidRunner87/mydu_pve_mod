using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Party.Data;
using NQ;

namespace Mod.DynamicEncounters.Features.Party.Interfaces;

public interface IPlayerPartyService
{
    Task<PartyOperationOutcome> CreateParty(PlayerId instigatorPlayerId);
    Task<PartyOperationOutcome> RequestJoinParty(PlayerId instigatorPlayerId, PlayerId targetPlayerId);
    Task<PartyOperationOutcome> RequestJoinParty(PlayerId instigatorPlayerId, string playerName);
    Task<PartyOperationOutcome> DisbandParty(PlayerId instigatorPlayerId);
    Task<PartyOperationOutcome> LeaveParty(PlayerId playerId);
    Task<PartyOperationOutcome> PromoteToPartyLeader(PlayerId instigatorPlayerId, PlayerId newLeaderPlayerId);
    Task<PartyOperationOutcome> PromoteToPartyLeader(PlayerId instigatorPlayerId, string playerName);
    Task<PartyOperationOutcome> InviteToParty(PlayerId instigatorPlayerId, PlayerId invitedPlayerId);
    Task<PartyOperationOutcome> InviteToParty(PlayerId instigatorPlayerId, string playerName);
    Task<PartyOperationOutcome> AcceptPartyInvite(PlayerId invitedPlayerId);
    Task<PartyOperationOutcome> CancelPartyInviteRequest(PlayerId instigatorPlayerId, PlayerId invitedPlayerId);
    Task<PartyOperationOutcome> CancelPartyInviteRequest(PlayerId instigatorPlayerId, string playerName);
    Task<PartyOperationOutcome> AcceptPartyRequest(PlayerId instigatorPlayerId, PlayerId invitedPlayerId);
    Task<PartyOperationOutcome> AcceptPartyRequest(PlayerId instigatorPlayerId, string playerName);
    Task<PartyOperationOutcome> KickPartyMember(PlayerId instigatorPlayerId, PlayerId invitedPlayerId);
    Task<PartyOperationOutcome> KickPartyMember(PlayerId instigatorPlayerId, string playerName);
    Task<PartyOperationOutcome> SetPlayerPartyRole(PlayerId instigatorPlayerId, string role);
    Task<IEnumerable<PlayerPartyItem>> GetPartyByPlayerId(PlayerId playerId);
}