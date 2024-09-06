using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Services;

public class ScriptActionFactory : IScriptActionFactory
{
    private readonly Dictionary<string,Func<ScriptActionItem,IScriptAction>> _actionMap;
    
    public ScriptActionFactory()
    {
        _actionMap = GetScriptActionMap();
    }

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

    public IEnumerable<string> GetAllActions() => _actionMap.Keys;

    private Dictionary<string, Func<ScriptActionItem, IScriptAction>> GetScriptActionMap()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(IScriptAction)))
            .Where(t => t.GetCustomAttribute<ScriptActionNameAttribute>() != null)
            .Where(t => t.GetConstructor([]) != null || t.GetConstructor([typeof(ScriptActionItem)]) != null);

        var dictionary = new Dictionary<string, Func<ScriptActionItem, IScriptAction>>();
        foreach (var type in types)
        {
            var key = type.GetCustomAttribute<ScriptActionNameAttribute>()!.Name;
            dictionary.TryAdd(key, actionItem =>
            {
                var actionItemConstructor = type.GetConstructor([typeof(ScriptActionItem)]);

                if (actionItemConstructor != null)
                {
                    return (IScriptAction)actionItemConstructor!.Invoke([actionItem]);
                }
                
                var defaultConstruct = type.GetConstructor([]);
                if (defaultConstruct != null)
                {
                    return (IScriptAction)defaultConstruct!.Invoke([]);
                }

                return new NullScriptAction();
            });
        }

        return dictionary;
    }

    private IScriptAction CreateInternalOrDefault(ScriptActionItem actionItem, IScriptAction action)
    {
        if (actionItem.Type != null && _actionMap.TryGetValue(actionItem.Type, out var value))
        {
            return value(actionItem);
        }
        
        switch (actionItem.Type)
        {
            case "test-combat":
                return new SpawnScriptAction(
                    new ScriptActionItem
                    {
                        Name = "spawn-test-encounter",
                        Area = new ScriptActionAreaItem(),
                        Type = "spawn",
                        Prefab = "test-enemy",
                        Position = actionItem.Position,
                        MinQuantity = 1,
                        MaxQuantity = 1
                    }
                );
            case "for-each-handle-with-tag":
                return new CompositeScriptAction(
                    Guid.NewGuid().ToString(),
                    actionItem.Tags
                        .Select(tag => new ForEachConstructHandleTaggedOnSectorAction(
                            tag,
                            Create(actionItem.Actions)
                        ))
                );
            case "random":
                var actions = actionItem
                    .Actions
                    .Select(a => CreateInternalOrDefault(a, new NullScriptAction()));

                return new RandomScriptAction(actions);
            default:
                return action;
        }
    }
}