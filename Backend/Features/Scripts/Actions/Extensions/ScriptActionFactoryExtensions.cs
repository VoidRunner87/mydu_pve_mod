using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Extensions;

public static class ScriptActionFactoryExtensions
{
    public static IScriptAction GetScriptAction(this IServiceProvider provider, ScriptActionItem item)
    {
        return provider.GetRequiredService<IScriptActionFactory>().Create(item);
    }
    
    public static IScriptAction GetScriptAction(this IServiceProvider provider, IEnumerable<ScriptActionItem> items)
    {
        return provider.GetRequiredService<IScriptActionFactory>().Create(items);
    }
}