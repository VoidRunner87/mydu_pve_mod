using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Services;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

[ScriptActionName(ActionName)]
public partial class TagSectorAsActiveScriptAction : IScriptAction
{
    public const string ActionName = "tag-sector-active";
    public string GetKey() => Name;

    public string Name => ActionName;

    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
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

        context.Properties.TryAdd("DelaySeconds", TimeSpan.FromMinutes(30).TotalSeconds);

        var provider = context.ServiceProvider;
        var constructHandlerRepo = provider.GetRequiredService<IConstructHandleRepository>();
        
        var result = (await constructHandlerRepo
            .FindTagInSectorAsync(context.Sector, "poi")).ToList();

        foreach (var handleItem in result)
        {
            var constructId = handleItem.ConstructId;
            var constructService = context.ServiceProvider.GetRequiredService<IConstructService>();
            var info = await constructService.GetConstructInfoAsync(constructId);

            if (!info.ConstructExists)
            {
                continue;
            }
        
            var name = ReplaceBetweenBracketsWithExclamation(info.Info!.rData.name);

            await constructService.RenameConstruct(constructId, name);
        }

        return await script.ExecuteAsync(context);
    }
    
    public static string ReplaceBetweenBracketsWithExclamation(string input)
    {
        return SquareBracketsContents().Replace(input, "[!!!]");
    }

    [GeneratedRegex(@"\[.*?\]")]
    private static partial Regex SquareBracketsContents();
}