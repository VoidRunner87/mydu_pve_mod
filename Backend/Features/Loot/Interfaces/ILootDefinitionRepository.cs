using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Loot.Data;

namespace Mod.DynamicEncounters.Features.Loot.Interfaces;

public interface ILootDefinitionRepository
{
    Task<IEnumerable<LootDefinitionItem>> GetAllActiveByAnyTagsAsync(IEnumerable<string> tags);
    Task<IEnumerable<LootDefinitionItem>> GetAllActiveByAllTagsAsync(IEnumerable<string> tags);
    Task<IEnumerable<LootDefinitionItem>> GetAllActiveTagsAsync(TagOperator tagOperator, IEnumerable<string> tags);
}