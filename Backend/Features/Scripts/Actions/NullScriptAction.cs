using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

public class NullScriptAction : IScriptAction
{
    public string Name => "null";

    public Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        context.ServiceProvider.CreateLogger<NullScriptAction>()
            .LogWarning("NULL ACTION");
        
        return Task.FromResult(ScriptActionResult.Successful());
    }

    public string GetKey()
    {
        return Name;
    }
}