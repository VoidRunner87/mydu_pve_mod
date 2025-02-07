using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Services;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

[ScriptActionName(ActionName)]
public class RandomChanceScriptAction(ScriptActionItem actionItem) : IScriptAction
{
    public const string ActionName = "chance-bool";
    public string GetKey() => Name;

    public string Name => ActionName;

    public Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var provider = context.ServiceProvider;
        var random = provider.GetRequiredService<IRandomProvider>()
            .GetRandom();
        var scriptActionFactory = provider.GetRequiredService<IScriptActionFactory>();

        var factor = random.NextSingle();
        var properties = actionItem.GetProperties<Properties>();

        if (factor > properties.GreaterThan)
        {
            var action = scriptActionFactory.Create(actionItem.Actions);
            return action.ExecuteAsync(context);
        }

        if (!properties.DefaultActions.Any())
        {
            return Task.FromResult(ScriptActionResult.Successful());
        }

        var defaultAction = scriptActionFactory.Create(properties.DefaultActions);
        return defaultAction.ExecuteAsync(context);
    }

    public class Properties
    {
        [JsonProperty] public float GreaterThan { get; set; } = 0.5f;
        [JsonProperty] public IEnumerable<ScriptActionItem> DefaultActions { get; set; } = [];
    }
}