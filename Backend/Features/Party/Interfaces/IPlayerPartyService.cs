using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Party.Data;
using NQ;

namespace Mod.DynamicEncounters.Features.Party.Interfaces;

public interface IPlayerPartyService
{
    Task<PartyOperationOutcome> CreateParty(PlayerId leaderPlayerId);
    Task<PartyOperationOutcome> RequestJoinParty(PlayerId leaderPlayerId, PlayerId memberPlayerId);
    Task<PartyOperationOutcome> DisbandParty(PlayerId leaderPlayerId);
    Task<PartyOperationOutcome> LeaveParty(PlayerId playerId);
    Task<PartyOperationOutcome> PromoteToPartyLeader(PlayerId leaderPlayerId, PlayerId newLeaderPlayerId);
    Task<PartyOperationOutcome> InviteToParty(PlayerId leaderPlayerId, PlayerId invitedPlayerId);

    Task<IEnumerable<PlayerPartyItem>> GetPartyByPlayerId(PlayerId playerId);
}