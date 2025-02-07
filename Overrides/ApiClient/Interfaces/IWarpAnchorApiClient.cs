using System.Threading.Tasks;
using Mod.DynamicEncounters.Overrides.ApiClient.Services;

namespace Mod.DynamicEncounters.Overrides.ApiClient.Interfaces;

public interface IWarpAnchorApiClient
{
    Task SetWarpEndCooldown(SetWarpEndCooldownRequest request);
}