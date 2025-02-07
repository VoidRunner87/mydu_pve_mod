using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.VoxelService.Data;
using NQ;

namespace Mod.DynamicEncounters.Features.VoxelService.Interfaces;

public interface IVoxelServiceClient
{
    Task TriggerConstructCacheAsync(ConstructId constructId);
    Task<QueryRandomPointOutcome> QueryRandomPoint(QueryRandomPoint query);
}