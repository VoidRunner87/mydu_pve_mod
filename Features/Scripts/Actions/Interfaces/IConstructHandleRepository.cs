using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Common.Repository;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using NQ;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

public interface IConstructHandleRepository : IRepository<ConstructHandleItem>
{
    Task<IEnumerable<ConstructHandleItem>> FindInSectorAsync(Vec3 sector);
    Task<IEnumerable<ConstructHandleItem>> FindExpiredAsync(int minutes, Vec3 sector);
    Task<ConstructHandleItem?> FindByConstructIdAsync(ulong constructId);

    Task UpdateLastControlledDateAsync(HashSet<ulong> constructIds);
}