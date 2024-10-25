using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Party.Data;
using NQ;

namespace Mod.DynamicEncounters.Features.Party.Interfaces;

public interface IPlayerPartyRepository
{
    Task<bool> IsInAParty(PlayerId playerId);
    Task<PlayerPartyGroupId> CreateParty(PlayerId leaderPlayerId);
    Task AddPendingPartyRequest(PlayerPartyGroupId groupId, PlayerId playerId);
    Task AddPendingPartyInvite(PlayerPartyGroupId groupId, PlayerId playerId);
    Task<PlayerPartyGroupId> FindPartyGroupId(PlayerId playerId);
    Task<bool> IsPartyLeader(PlayerPartyGroupId groupId, PlayerId playerId);
    Task DisbandParty(PlayerPartyGroupId groupId);
    Task RemoveNonLeaderPlayerFromParty(PlayerPartyGroupId groupId, PlayerId playerId);
    Task RemovePlayerFromPartyAndFindNewLeader(PlayerPartyGroupId groupId, PlayerId playerId);
    Task SetPartyLeader(PlayerPartyGroupId groupId, PlayerId playerId);
    Task<IEnumerable<PlayerPartyItem>> GetPartyByPlayerId(PlayerId playerId);
}