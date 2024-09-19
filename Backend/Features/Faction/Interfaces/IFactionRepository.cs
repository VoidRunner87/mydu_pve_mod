using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Faction.Data;

namespace Mod.DynamicEncounters.Features.Faction.Interfaces;

public interface IFactionRepository
{
    Task<IEnumerable<FactionItem>> GetAllAsync();
}