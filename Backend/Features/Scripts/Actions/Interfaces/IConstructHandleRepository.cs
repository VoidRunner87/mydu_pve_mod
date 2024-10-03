using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Common.Repository;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using NQ;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

public interface IConstructHandleRepository : IRepository<ConstructHandleItem>
{
    Task<IEnumerable<ConstructHandleItem>> FindTagInSectorAsync(Vec3 sector, string tag);
    Task<IEnumerable<ConstructHandleItem>> FindInSectorAsync(Vec3 sector);
    Task<ConstructHandleItem?> FindByConstructIdAsync(ulong constructId);
    Task<IEnumerable<ConstructHandleItem>> FindActiveHandlesAsync();
    Task<IEnumerable<ulong>> FindAllBuggedPoiConstructsAsync();
    Task DeleteByConstructId(ulong constructId);

    Task UpdateLastControlledDateAsync(HashSet<ulong> constructIds);
    Task RemoveHandleAsync(ulong constructId);

    Task<Dictionary<ulong, TimeSpan>> GetPoiConstructExpirationTimeSpansAsync();
    
    Task TagAsDeletedConstructHandledThatAreDeletedConstructs();
}