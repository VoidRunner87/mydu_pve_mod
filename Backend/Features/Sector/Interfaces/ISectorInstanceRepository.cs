using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Common.Repository;
using Mod.DynamicEncounters.Features.Sector.Data;
using NQ;

namespace Mod.DynamicEncounters.Features.Sector.Interfaces;

public interface ISectorInstanceRepository : IRepository<SectorInstance>
{
    Task<SectorInstance?> FindById(Guid id);
    Task<SectorInstance?> FindBySector(Vec3 sector);
    Task<IEnumerable<SectorInstance>> FindExpiredAsync();
    Task DeleteExpiredAsync();
    Task SetExpirationFromNowAsync(Guid id, TimeSpan span);
    Task<IEnumerable<SectorInstance>> FindUnloadedAsync();
    Task SetLoadedAsync(Guid id, bool loaded);
    Task TagAsStartedAsync(Guid id);
    Task<IEnumerable<SectorInstance>> FindSectorsRequiringStartupAsync();
    Task<IEnumerable<SectorInstance>> ScanForInactiveSectorsVisitedByPlayers(double distance);
    Task ExpireAllAsync();
    Task ForceExpireAllAsync();
    Task<long> GetCountWithTagAsync(string tag);
    Task<long> GetCountByTerritoryAsync(Guid territoryId);
    Task ExpireSectorsWithDeletedConstructHandles();
}