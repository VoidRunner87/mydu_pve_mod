using System.Collections.Generic;
using System.Threading.Tasks;
using NQ;

namespace Mod.DynamicEncounters.Features.Common.Interfaces;

public interface IConstructSpatialHashRepository
{
    Task<IEnumerable<ulong>> FindConstructsOnSector(Vec3 sector);
}