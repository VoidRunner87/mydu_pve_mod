using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Common;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Services;

public class RandomScriptAction(IEnumerable<IScriptAction> actions) : IScriptAction
{
    public string GetKey() => Name;

    public string Name => Guid.NewGuid().ToString();
    public Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var provider = context.ServiceProvider;
        var random = provider.GetRequiredService<IRandomProvider>()
            .GetRandom();

        var action = random.PickOneAtRandom(actions);

        return action.ExecuteAsync(context);
    }
}