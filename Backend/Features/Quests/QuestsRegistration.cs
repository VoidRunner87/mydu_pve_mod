using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Quests.Interfaces;
using Mod.DynamicEncounters.Features.Quests.Repository;
using Mod.DynamicEncounters.Features.Quests.Services;

namespace Mod.DynamicEncounters.Features.Quests;

public static class QuestsRegistration
{
    public static void RegisterQuests(this IServiceCollection services)
    {
        services.AddSingleton<ITerritoryContainerRepository, TerritoryContainerRepository>();
        services.AddSingleton<IProceduralQuestGeneratorService, ProceduralQuestGeneratorService>();
        services.AddSingleton<IProceduralTransportMissionGeneratorService, ProceduralTransportMissionGeneratorService>();
        services.AddSingleton<IProceduralReverseTransportMissionGeneratorService, ProceduralReverseTransportMissionGeneratorService>();
        services.AddSingleton<IPlayerQuestService, PlayerQuestService>();
        services.AddSingleton<IPlayerQuestRepository, PlayerQuestRepository>();
        services.AddSingleton<ITransportMissionTemplateProvider, TransportMissionTemplateProvider>();
        services.AddSingleton<IQuestInteractionService, QuestInteractionService>();
        services.AddSingleton<IProceduralLootBasedMissionGeneratorService, ProceduralLootBasedMissionGeneratorService>();
    }
}