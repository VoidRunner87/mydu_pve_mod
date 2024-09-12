using System.Collections.Generic;

namespace Mod.DynamicEncounters.Features.Loot.Interfaces;

public class LootGenerationArgs
{
    public long MaxBudget { get; set; } = 1;
    public IEnumerable<string> Tags { get; set; } = [];
}