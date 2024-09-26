using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Common.Repository;
using NQ;

namespace Mod.DynamicEncounters.Features.Common.Interfaces;

public interface IConstructSpatialHashRepository
{
    Task<IEnumerable<ulong>> FindPlayerLiveConstructsOnSector(Vec3 sector);
    Task<IEnumerable<ConstructSpatialHashRepository.ConstructSectorRow>> FindPlayerLiveConstructsOnSectorInstances();
}