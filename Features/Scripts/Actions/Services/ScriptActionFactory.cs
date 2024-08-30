using System;
using System.Collections.Generic;
using System.Linq;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;

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
            default:
                return action;
        }
    }
}