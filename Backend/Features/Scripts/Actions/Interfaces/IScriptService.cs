using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

public interface IScriptService
{
    Task<ScriptActionResult> ExecuteScriptAsync(string name, ScriptContext context);
}