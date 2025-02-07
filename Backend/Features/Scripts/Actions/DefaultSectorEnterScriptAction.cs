using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Services;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

[ScriptActionName(ActionName)]
public class DefaultSectorEnterScriptAction : IScriptAction
{
    public const string ActionName = "default-sector-enter";
    public string GetKey() => Name;

    public string Name => ActionName;

    public Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var script = new CompositeScriptAction([
            new ForEachConstructHandleTaggedOnSectorAction(
                "poi",
                new DelayedScriptAction(
                    new ScriptActionItem
                    {
                        Actions = [
                            new ScriptActionItem
                            {
                                Type = "delete"
                            }
                        ]
                    }
                )
            )
        ]);

        return script.ExecuteAsync(context);
    }
}