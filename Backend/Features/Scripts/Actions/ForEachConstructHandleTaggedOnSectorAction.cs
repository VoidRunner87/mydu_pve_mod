using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

/// <summary>
/// Runs a script action for each construct handle still registered in the sector that is tagged by the "tag" param.
/// </summary>
/// <param name="sector"></param>
/// <param name="tag"></param>
/// <param name="scriptAction"></param>
public class ForEachConstructHandleTaggedOnSectorAction(
    string tag,
    IScriptAction scriptAction
) : IScriptAction
{
    public string GetKey() => Name;

    public string Name { get; } = Guid.NewGuid().ToString();
    
    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var provider = context.ServiceProvider;
        var constructHandlerRepo = provider.GetRequiredService<IConstructHandleRepository>();
        var logger = provider.CreateLogger<ForEachConstructHandleTaggedOnSectorAction>();
        
        var result = (await constructHandlerRepo
            .FindTagInSectorAsync(context.Sector, tag)).ToList();
        
        logger.LogInformation("Query yield '{Count}' construct handles", result.Count);

        foreach (var handleItem in result)
        {
            var itemContext = new ScriptContext(
                context.ServiceProvider,
                context.PlayerIds,
                context.Sector
            )
            {
                ConstructId = handleItem.ConstructId
            };

            await scriptAction.ExecuteAsync(itemContext);
        }
        
        return ScriptActionResult.Successful();
    }
}