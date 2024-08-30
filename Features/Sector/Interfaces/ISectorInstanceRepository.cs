using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Common.Repository;
using Mod.DynamicEncounters.Features.Sector.Data;

namespace Mod.DynamicEncounters.Features.Sector.Interfaces;

public interface ISectorInstanceRepository : IRepository<SectorInstance>
{
    public Task<IEnumerable<SectorInstance>> FindExpiredAsync();
    public Task DeleteExpiredAsync();
    public Task<IEnumerable<SectorInstance>> FindUnloadedAsync();
    public Task SetLoadedAsync(Guid id, bool loaded);
    public Task TagAsStartedAsync(Guid id);
    public Task<IEnumerable<SectorInstance>> FindSectorsRequiringStartupAsync();
}