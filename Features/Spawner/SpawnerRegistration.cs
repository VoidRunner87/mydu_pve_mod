using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Common.Repository;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Repository;
using Mod.DynamicEncounters.Features.Scripts.Actions.Services;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Services;

namespace Mod.DynamicEncounters.Features.Spawner;

public static class SpawnerRegistration
{
    public static void RegisterSpawnerScripts(this IServiceCollection services)
    {
        services.AddSingleton<IScriptService, ScriptService>();
        services.AddSingleton<IScriptActionFactory, ScriptActionFactory>();
        services.AddSingleton<IConstructDefinitionFactory, ConstructDefinitionFactory>();
        services.AddSingleton<IPointGeneratorFactory, PointGeneratorFactory>();
        services.AddSingleton<IConstructBehaviorFactory, ConstructBehaviorFactory>();
        
        services.AddSingleton<IRepository<IScriptAction>, ScriptActionMemoryRepository>();
        services.AddSingleton<IScriptActionItemRepository, ScriptActionItemDatabaseRepository>();
        services.AddSingleton<IConstructDefinitionItemRepository, ConstructDefinitionItemDatabaseRepository>();
        services.AddSingleton<IRepository<IConstructHandle>, ConstructHandleMemoryRepository>();
        services.AddSingleton<IRepository<IConstructDefinition>, ConstructDefinitionMemoryRepository>();
        services.AddSingleton<IConstructHandleRepository, ConstructHandleDatabaseRepository>();
    }
}