using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Faction.Data;
using Mod.DynamicEncounters.Features.Quests.Data;

namespace Mod.DynamicEncounters.Features.Quests.Interfaces;

public interface ITerritoryContainerRepository
{
    Task<IEnumerable<TerritoryContainerItem>> GetAll(
        TerritoryId territoryId
    );

    Task Add(Guid territoryId, ulong constructId, ulong elementId);
}