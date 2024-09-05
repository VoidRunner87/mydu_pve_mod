using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Features.Scripts.Interfaces;

public interface IScriptLoaderService
{
    Task<IScriptAction> LoadScriptAction(string filePath);
    IScriptAction LoadScript(ScriptActionItem item);
    IConstructDefinition LoadScript(PrefabItem item);
    Task<IConstructDefinition> LoadConstructDefinition(string filePath);
}