using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Warp.Data;

namespace Mod.DynamicEncounters.Features.Warp.Interfaces;

public interface IWarpAnchorService
{
    Task<CreateWarpAnchorOutcome> SpawnWarpAnchor(SpawnWarpAnchorCommand command);
    Task<CreateWarpAnchorOutcome> CreateWarpAnchorForPosition(CreateWarpAnchorCommand command);
    Task<CreateWarpAnchorOutcome> CreateWarpAnchorForward(CreateWarpAnchorForwardCommand command);
    Task<SetWarpCooldownOutcome> SetWarpCooldown(SetWarpCooldownCommand command);
}