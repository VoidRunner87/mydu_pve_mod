using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

public class NullScriptAction : IScriptAction
{
    public string Name { get; }

    public Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        return Task.FromResult(ScriptActionResult.Successful());
    }

    public string GetKey()
    {
        return Name;
    }
}