using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Interfaces;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

public class ExpireSectorAction(TimeSpan timeSpan) : IScriptAction
{
    public string GetKey() => Name;

    public string Name { get; } = Guid.NewGuid().ToString();
    
    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var provider = context.ServiceProvider;

        var sectorPoolManager = provider.GetRequiredService<ISectorPoolManager>();
        await sectorPoolManager.SetExpirationFromNow(context.Sector, timeSpan);
        
        return ScriptActionResult.Successful();
    }
}