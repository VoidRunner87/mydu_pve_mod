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
    private readonly IScriptActionItemRepository _scriptActionItemActionRepository = 
        serviceProvider.GetRequiredService<IScriptActionItemRepository>();
    private readonly IPrefabItemRepository _prefabItemRepository = 
        serviceProvider.GetRequiredService<IPrefabItemRepository>();

    public async Task LoadAllFromDatabase()
    {
        var scriptActionRepo = serviceProvider.GetRequiredService<IRepository<IScriptAction>>();
        var constructDefRepo = serviceProvider.GetRequiredService<IRepository<IPrefab>>();
        var scriptsTask = _scriptActionItemActionRepository.GetAllAsync();
        var constructDefsTask = _prefabItemRepository.GetAllAsync();

        await Task.WhenAll(scriptsTask, constructDefsTask);
        //TODO This is accumulating scripts because they are guid named
        var scripts = await scriptsTask;
        var constructDefs = await constructDefsTask;

        var addTasksScript = scripts
            .Select(_scriptLoaderService.LoadScript)
            .Select(scriptActionRepo.AddAsync);

        var addTasksConstructDef = constructDefs
            .Select(_scriptLoaderService.LoadScript)
            .Select(constructDefRepo.AddAsync);

        await Task.WhenAll(
            Task.WhenAll(addTasksScript),
            Task.WhenAll(addTasksConstructDef)
        );
    }

    public Task LoadAll(string basePath, string folderPath)
    {
        return Task.WhenAll(
            LoadAllScriptsActionsAsync(basePath, folderPath),
            LoadAllConstructDefinitionsAsync(basePath, folderPath)
        );
    }

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

    private async Task LoadAllScriptsActionsAsync(string basePath, string folderPath)
    {
        var repository = serviceProvider.GetRequiredService<IRepository<IScriptAction>>();

        var assembly = Assembly.GetExecutingAssembly();
        var location = Path.GetDirectoryName(assembly.Location)!;

        var configPath = Path.Combine(location, basePath, folderPath, "Scripts");
        
        Console.WriteLine($"Searching Folder '{configPath}'");
        _logger.LogInformation("Searching Folder {Folder}", configPath);

        var files = Directory.GetFiles(configPath, searchPattern: "*.yaml");
        
        var loadTasks = files.Select(_scriptLoaderService.LoadScriptAction);

        var result = await Task.WhenAll(loadTasks);
        
        _logger.LogInformation("Loaded Scripts: {ScriptList}: \n", string.Join(Environment.NewLine, files));

        await repository.AddRangeAsync(result);
    }

    private async Task LoadAllConstructDefinitionsAsync(string basePath, string folderPath)
    {
        var repository = serviceProvider.GetRequiredService<IRepository<IPrefab>>();

        var assembly = Assembly.GetExecutingAssembly();
        var location = Path.GetDirectoryName(assembly.Location)!;

        var configPath = Path.Combine(location, basePath, folderPath, "Scripts/Prefabs");
        
        Console.WriteLine($"Searching Folder '{configPath}'");
        _logger.LogInformation("Searching Folder {Folder}", configPath);

        var files = Directory.GetFiles(configPath, searchPattern: "*.yaml");
        
        var loadTasks = files.Select(_scriptLoaderService.LoadConstructDefinition);

        var result = await Task.WhenAll(loadTasks);
        
        _logger.LogInformation("Loaded Construct Definitions: {ScriptList}: \n", string.Join(Environment.NewLine, files));

        await repository.AddRangeAsync(result);
    }
}