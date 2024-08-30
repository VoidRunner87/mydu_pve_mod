using System;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

namespace Mod.DynamicEncounters.Features.Spawner.Data;

public class ConstructDefinition(
    ConstructDefinitionItem definitionItem
) : IConstructDefinition
{
    public Guid Id => DefinitionItem.Id;
    public ConstructDefinitionItem DefinitionItem { get; } = definitionItem;
    public IConstructEvents Events { get; init; } = new ConstructEvents();

    public string GetKey()
    {
        return DefinitionItem.Name;
    }
}