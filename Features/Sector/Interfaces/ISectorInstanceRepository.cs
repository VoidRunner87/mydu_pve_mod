using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Common.Repository;
using Mod.DynamicEncounters.Features.Sector.Data;
using NQ;

namespace Mod.DynamicEncounters.Features.Sector.Interfaces;

public interface ISectorInstanceRepository : IRepository<SectorInstance>
{
    public Task<SectorInstance?> FindBySector(Vec3 sector);
    public Task<IEnumerable<SectorInstance>> FindExpiredAsync();
    public Task DeleteExpiredAsync();
    public Task ExtendExpirationAsync(Guid id, int minutes);
    public Task<IEnumerable<SectorInstance>> FindUnloadedAsync();
    public Task SetLoadedAsync(Guid id, bool loaded);
    public Task TagAsStartedAsync(Guid id);
    public Task<IEnumerable<SectorInstance>> FindSectorsRequiringStartupAsync();
}