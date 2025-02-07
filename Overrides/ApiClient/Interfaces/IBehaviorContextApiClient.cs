using System.Threading;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Overrides.ApiClient.Services;

namespace Mod.DynamicEncounters.Overrides.ApiClient.Interfaces;

public interface IBehaviorContextApiClient
{
    Task RegisterDamage(RegisterDamageRequest request, CancellationToken cancellationToken = default);
}