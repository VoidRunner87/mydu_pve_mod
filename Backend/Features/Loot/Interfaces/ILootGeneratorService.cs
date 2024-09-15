﻿using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Loot.Data;

namespace Mod.DynamicEncounters.Features.Loot.Interfaces;

public interface ILootGeneratorService
{
    Task<ItemBagData> GenerateAsync(LootGenerationArgs args);
}