using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

public interface IScriptAction : IHasKey<string>
{
    string Name { get; }
    
    Task<ScriptActionResult> ExecuteAsync(ScriptContext context);
}