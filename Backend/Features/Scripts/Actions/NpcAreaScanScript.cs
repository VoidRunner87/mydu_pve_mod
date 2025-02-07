using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Services;
using Mod.DynamicEncounters.Helpers;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

[ScriptActionName(ActionName, Description = Description)]
public class NpcAreaScanScript(ScriptActionItem actionItem) : IScriptAction
{
    public const string ActionName = "npc-area-scan";
    public const string Description = "NPC Area Scan with script outputs per condition";
    
    public string Name => ActionName;
    public string GetKey() => Name;
    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var provider = context.ServiceProvider;
        var areaScanService = provider.GetRequiredService<IAreaScanService>(); 
        var actionFactory = provider.GetRequiredService<IScriptActionFactory>();

        var properties = actionItem.GetProperties<Properties>();

        var contacts = (await areaScanService.ScanForNpcConstructs(context.Sector, properties.ScanRadius))
            .ToList();
        if (contacts.Count > 0)
        {
            var hasContactsAction = actionFactory.Create(properties.HasContacts);
            await hasContactsAction.ExecuteAsync(context);
        }
        else
        {
            var noContactsAction = actionFactory.Create(properties.NoContacts);
            await noContactsAction.ExecuteAsync(context);
        }
        
        return ScriptActionResult.Successful();
    }

    public class Properties
    {
        [JsonProperty] public double ScanRadius { get; set; } = DistanceHelpers.OneSuInMeters * 2;
        [JsonProperty] public IEnumerable<ScriptActionItem> HasContacts { get; set; } = [];
        [JsonProperty] public IEnumerable<ScriptActionItem> NoContacts { get; set; } = [];
    }
}