﻿using System;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

namespace Mod.DynamicEncounters.Features.Spawner.Data;

public class Prefab(
    PrefabItem definitionItem
) : IPrefab
{
    public Guid Id => DefinitionItem.Id;
    public PrefabItem DefinitionItem { get; } = definitionItem;
    public IConstructEvents Events { get; init; } = new ConstructEvents();

    public string GetKey()
    {
        return DefinitionItem.Name;
    }
}