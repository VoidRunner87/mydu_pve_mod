using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Services;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

[ScriptActionName(ActionName, Description = Description)]
public class RemoveConstructBuffsAction(ScriptActionItem actionItem) : IScriptAction
{
    public ScriptActionItem ActionItem { get; } = actionItem;
    public const string ActionName = "remove-buffs";
    public const string Description = "Removes Construct's Buffs";
    
    public string Name => ActionName;
    public string GetKey() => Name;
    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        if (!context.ConstructId.HasValue)
        {
            return ScriptActionResult.Failed();
        }
        
        var provider = context.ServiceProvider;

        var orleans = provider.GetOrleans();
        var modManagerGrain = orleans.GetModManagerGrain();
        await modManagerGrain.TriggerModAction(
            ModBase.Bot.PlayerId,
            new ModAction
            {
                modName = "Mod.DynamicEncounters",
                actionId = 1000008,
                constructId = context.ConstructId.Value
            }
        );

        return ScriptActionResult.Successful();
    }
}