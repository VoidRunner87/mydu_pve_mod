using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Overrides.ApiClient.Data;

namespace Mod.DynamicEncounters.Overrides.ApiClient.Interfaces;

public interface IPveModPartyApiClient
{
    Task<BasicOutcome> KickPartyMember(ulong instigatorPlayerId, ulong playerId, CancellationToken cancellationToken);
    Task<BasicOutcome> SetPartyRole(ulong instigatorPlayerId, string role, CancellationToken cancellationToken);
    Task<BasicOutcome> PromoteToLeader(ulong instigatorPlayerId, ulong playerId, CancellationToken cancellationToken);
    Task<BasicOutcome> AcceptRequest(ulong instigatorPlayerId, ulong playerId, CancellationToken cancellationToken);
    Task<BasicOutcome> RejectRequest(ulong instigatorPlayerId, ulong playerId, CancellationToken cancellationToken);
    Task<BasicOutcome> AcceptInvite(ulong instigatorPlayerId, CancellationToken cancellationToken);

    Task<BasicOutcome> SendPartyInvite(
        ulong instigatorPlayerId,
        ulong playerId,
        string playerName,
        CancellationToken cancellationToken
    );

    Task<BasicOutcome> CancelInvite(ulong instigatorPlayerId, ulong playerId, CancellationToken cancellationToken);
    Task<BasicOutcome> CreateParty(ulong instigatorPlayerId, CancellationToken cancellationToken);
    Task<BasicOutcome> LeaveParty(ulong instigatorPlayerId, CancellationToken cancellationToken);
    Task<BasicOutcome> DisbandParty(ulong instigatorPlayerId, CancellationToken cancellationToken);
    Task<IEnumerable<PlayerPartyItem>> GetPartyByPlayerId(ulong playerId, CancellationToken cancellationToken);
}