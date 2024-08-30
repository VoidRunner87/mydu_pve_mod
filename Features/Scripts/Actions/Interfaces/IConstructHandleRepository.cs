using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Common.Repository;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

public interface IConstructHandleRepository : IRepository<ConstructHandleItem>
{
    Task<IEnumerable<ConstructHandleItem>> FindExpiredAsync(int minutes);

    Task UpdateLastControlledDateAsync(HashSet<ulong> constructIds);
}