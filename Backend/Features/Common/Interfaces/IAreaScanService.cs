using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Common.Data;
using NQ;

namespace Mod.DynamicEncounters.Features.Common.Interfaces;

public interface IAreaScanService
{
    Task<IEnumerable<ScanContact>> ScanForPlayerContacts(
        ulong constructId,
        Vec3 position,
        double radius,
        int limit = 5
    );
    
    Task<IEnumerable<ScanContact>> ScanForNpcConstructs(Vec3 position, double radius, int limit = 10);
    Task<IEnumerable<ScanContact>> ScanForAbandonedConstructs(Vec3 position, double radius, int limit = 10);

    Task<IEnumerable<ScanContact>> ScanForAsteroids(Vec3 position, double radius);
    Task<IEnumerable<ScanContact>> ScanForPlanetaryBodies(Vec3 position, double radius);
}