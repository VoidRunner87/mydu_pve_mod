using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Common.Data;
using NQ;

namespace Mod.DynamicEncounters.Features.Common.Interfaces;

public interface INpcRadarService
{
    Task<IEnumerable<NpcRadarContact>> ScanForPlayerContacts(
        ulong constructId,
        Vec3 position,
        double radius,
        int limit = 5
    );
}