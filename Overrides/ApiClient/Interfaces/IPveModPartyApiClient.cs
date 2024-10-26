using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Overrides.ApiClient.Data;

namespace Mod.DynamicEncounters.Overrides.ApiClient.Interfaces;

public interface IPveModPartyApiClient
{
    Task<IEnumerable<PlayerPartyItem>> GetPartyByPlayerId(ulong playerId, CancellationToken cancellationToken);
}