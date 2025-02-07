using System;
using System.Collections.Generic;
using Mod.DynamicEncounters.Features.Loot.Data;

namespace Mod.DynamicEncounters.Features.Loot.Interfaces;

public class LootGenerationArgs
{
    public double MaxBudget { get; set; } = 1;
    public IEnumerable<string> Tags { get; set; } = [];
    public int Seed { get; set; } = new Random().Next();
    public TagOperator Operator { get; set; } = TagOperator.AnyTags;
}