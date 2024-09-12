using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Loot.Data;

namespace Mod.DynamicEncounters.Features.Loot.Interfaces;

public interface ILootDefinitionRepository
{
    Task<IEnumerable<LootDefinitionItem>> GetAllActiveByTagsAsync(IEnumerable<string> tags);
}