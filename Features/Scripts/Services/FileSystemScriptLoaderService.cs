using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Common;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Features.Scripts.Services;

public class FileSystemScriptLoaderService(IServiceProvider serviceProvider) : IScriptLoaderService
{
    private readonly ILogger<FileSystemScriptLoaderService> _logger = serviceProvider.CreateLogger<FileSystemScriptLoaderService>();
    private readonly IScriptActionFactory _scriptActionFactory = serviceProvider.GetRequiredService<IScriptActionFactory>();
    private readonly IConstructDefinitionFactory _constructDefinitionFactory = serviceProvider.GetRequiredService<IConstructDefinitionFactory>();
    
    public async Task<IScriptAction> LoadScriptAction(string filePath)
    {
        try
        {
            var deserializer = serviceProvider.GetRequiredService<IYamlDeserializer>();

            var action = deserializer.Deserialize<ScriptActionItem>(await File.ReadAllTextAsync(filePath));
            action.Name = Path.GetFileName(filePath);

            return LoadScript(action);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to Load ScriptAction: {filePath}", filePath);
            
            throw;
        }
    }

    public IScriptAction LoadScript(ScriptActionItem item)
    {
        return _scriptActionFactory.Create(item);
    }

    public IConstructDefinition LoadScript(ConstructDefinitionItem item)
    {
        return _constructDefinitionFactory.Create(item);
    }

    public async Task<IConstructDefinition> LoadConstructDefinition(string filePath)
    {
        try
        {
            var deserializer = serviceProvider.GetRequiredService<IYamlDeserializer>();

            var action = deserializer.Deserialize<ConstructDefinitionItem>(await File.ReadAllTextAsync(filePath));

            var factory = serviceProvider.GetRequiredService<IConstructDefinitionFactory>();

            return factory.Create(action);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to Load ConstructDefinition: {filePath}", filePath);
            
            throw;
        }
    }
}