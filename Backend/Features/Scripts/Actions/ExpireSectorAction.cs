using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Services;
using Mod.DynamicEncounters.Features.Sector.Interfaces;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

[ScriptActionName(ActionName)]
public class ExpireSectorAction : IScriptAction
{
    public const string ActionName = "expire-sector";
    
    public string GetKey() => Name;

    public string Name => ActionName;

    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var provider = context.ServiceProvider;

        var sectorPoolManager = provider.GetRequiredService<ISectorPoolManager>();
        await sectorPoolManager.SetExpirationFromNow(context.Sector, TimeSpan.FromHours(1));
        
        return ScriptActionResult.Successful();
    }
}