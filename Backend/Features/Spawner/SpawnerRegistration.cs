using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Common.Repository;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Repository;
using Mod.DynamicEncounters.Features.Scripts.Actions.Services;
using Mod.DynamicEncounters.Features.Scripts.Validators;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Repository;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Services;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Services;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Features.Spawner.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Services;
using Mod.DynamicEncounters.Features.Spawner.Validators;

namespace Mod.DynamicEncounters.Features.Spawner;

public static class SpawnerRegistration
{
    public static void RegisterSpawnerScripts(this IServiceCollection services)
    {
        services.AddSingleton<IValidator<ScriptActionItem>, ScriptActionItemValidator>();
        services.AddSingleton<IValidator<PrefabItem>, PrefabItemValidator>();
        
        services.AddSingleton<IScriptService, ScriptService>();
        services.AddSingleton<IScriptActionFactory, ScriptActionFactory>();
        services.AddSingleton<IConstructDefinitionFactory, ConstructDefinitionFactory>();
        services.AddSingleton<IPointGeneratorFactory, PointGeneratorFactory>();
        services.AddSingleton<IConstructBehaviorFactory, ConstructBehaviorFactory>();
        services.AddSingleton<IConstructInMemoryBehaviorContextRepository, ConstructInMemoryBehaviorContextRepository>();
        services.AddSingleton<IConstructStateRepository, ConstructStateRepository>();
        services.AddSingleton<IConstructStateService, ConstructStateService>();
        services.AddSingleton<ITravelRouteService, TravelRouterService>();
        services.AddSingleton<IAsteroidSpawnerService, AsteroidSpawnerService>();
        
        services.AddSingleton<IScriptActionItemRepository, ScriptActionItemDatabaseRepository>();
        services.AddSingleton<IPrefabItemRepository, PrefabItemDatabaseRepository>();
        services.AddSingleton<IConstructHandleRepository, ConstructHandleDatabaseRepository>();
        services.AddSingleton<ISkillFactory, SkillFactory>();
        services.AddSingleton<IJamTargetService, JamTargetService>();
    }
}