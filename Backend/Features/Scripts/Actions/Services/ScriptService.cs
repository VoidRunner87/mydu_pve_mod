using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Common.Repository;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Services;

public class ScriptService(IServiceProvider serviceProvider) : IScriptService
{
    private readonly IScriptLoaderService _scriptLoaderService = 
        serviceProvider.GetRequiredService<IScriptLoaderService>();

    private readonly ILogger<ScriptService> _logger = serviceProvider.CreateLogger<ScriptService>();

    public async Task<ScriptActionResult> ExecuteScriptAsync(string name, ScriptContext context)
    {
        if (string.IsNullOrEmpty(name))
        {
            _logger.LogInformation("No script to run for the sector {Sector}", context.Sector);
            return ScriptActionResult.Successful();
        }
        
        var repository = serviceProvider.GetRequiredService<IScriptActionItemRepository>();

        var scriptAction = await repository.FindAsync(name);
        if (scriptAction == null)
        {
            _logger.LogError("Script {Name} not found or failed to load.", name);
            
            return ScriptActionResult.Failed()
                .WithMessage($"Script '{name}' not found or failed to load.");
        }

        var scriptActionFactory = serviceProvider.GetRequiredService<IScriptActionFactory>();
        var action = scriptActionFactory.Create(scriptAction);

        return await action.ExecuteAsync(context);
    }
}