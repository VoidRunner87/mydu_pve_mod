using System;
using System.Collections.Generic;
using System.Linq;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Services;

public class ScriptActionFactory : IScriptActionFactory
{
    public IScriptAction Create(ScriptActionItem scriptActionItem)
    {
        return CreateInternalOrDefault(
            scriptActionItem,
            new CompositeScriptAction(
                scriptActionItem.Name!,
                scriptActionItem.Actions.Select(Create)
            )
        );
    }

    public IScriptAction Create(IEnumerable<ScriptActionItem> scriptActionItem)
    {
        var actions = scriptActionItem
            .Select(a => CreateInternalOrDefault(a, new NullScriptAction()));

        return new CompositeScriptAction(Guid.NewGuid().ToString(), actions);
    }

    private IScriptAction CreateInternalOrDefault(ScriptActionItem scriptActionItem, IScriptAction action)
    {
        switch (scriptActionItem.Type)
        {
            case "chat-dm":
                return new ChatDmScriptAction(scriptActionItem.Message);
            case "spawn":
                return new SpawnScriptAction(scriptActionItem);
            case "run":
                return new RunScriptAction(scriptActionItem.Script);
            case "delete-construct":
                return new DeleteConstructAction(scriptActionItem.ConstructId);
            case "despawn":
                return new DespawnNpcConstructAction(scriptActionItem.ConstructId);
            case "despawn-wreck":
                return new DespawnWreckConstructAction(scriptActionItem.ConstructId);
            case "test-combat":
                return new SpawnScriptAction(
                    new ScriptActionItem
                    {
                        Name = "spawn-test-encounter",
                        Area = new ScriptActionAreaItem(),
                        Type = "spawn",
                        Prefab = "test-enemy",
                        Position = scriptActionItem.Position,
                        MinQuantity = 1,
                        MaxQuantity = 1
                    }
                );
            case "remove-poi":
            case "deactivate-dynamic-wreck":
                return new DeactivateDynamicWreckAction();
            case "for-each-handle-with-tag":
                return new CompositeScriptAction(
                    Guid.NewGuid().ToString(),
                    scriptActionItem.Tags
                        .Select(tag => new ForEachConstructHandleTaggedOnSectorAction(
                            tag,
                            Create(scriptActionItem.Actions)
                        ))
                );
            case "expire-sector":
                return new ExpireSectorAction(scriptActionItem.TimeSpan);
            case "random":
                var actions = scriptActionItem
                    .Actions
                    .Select(a => CreateInternalOrDefault(a, new NullScriptAction()));

                return new RandomScriptAction(actions);
            default:
                if (string.IsNullOrEmpty(action.Name))
                {
                    throw new InvalidOperationException("A composite action (action with multiple actions) need a name if the are root on the script");
                }
                
                return action;
        }
    }
}