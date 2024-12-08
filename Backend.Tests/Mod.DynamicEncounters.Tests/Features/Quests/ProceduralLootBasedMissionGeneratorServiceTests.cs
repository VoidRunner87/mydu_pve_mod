﻿using Backend;
using Backend.Scenegraph;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Faction.Data;
using Mod.DynamicEncounters.Features.Faction.Interfaces;
using Mod.DynamicEncounters.Features.Loot.Data;
using Mod.DynamicEncounters.Features.Loot.Interfaces;
using Mod.DynamicEncounters.Features.Market.Data;
using Mod.DynamicEncounters.Features.Market.Interfaces;
using Mod.DynamicEncounters.Features.Quests.Data;
using Mod.DynamicEncounters.Features.Quests.Interfaces;
using Mod.DynamicEncounters.Features.Quests.Services;
using NQ;
using NSubstitute;

namespace Mod.DynamicEncounters.Tests.Features.Quests;

[TestFixture]
public class ProceduralLootBasedMissionGeneratorServiceTests
{
    [Test]
    public void Should_Generate_Procedural_Loot_Quest()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var constructService = Substitute.For<IConstructService>();
        constructService.GetConstructInfoAsync(Arg.Any<ulong>())
            .Returns(new ConstructInfoOutcome(true, new ConstructInfo()));
        constructService.IsInSafeZone(Arg.Any<ulong>())
            .Returns(false);
        services.AddSingleton(constructService);

        var lootGeneratorService = Substitute.For<ILootGeneratorService>();
        lootGeneratorService.GenerateAsync(Arg.Any<LootGenerationArgs>())
            .Returns(new ItemBagData(1000)
            {
                CurrentCost = 1,
                Entries =
                [
                    new ItemBagData.ItemAndQuantity("WeaponRailgun1", new DefaultQuantity(1)),
                    new ItemBagData.ItemAndQuantity("WeaponRailgun2", new DefaultQuantity(2)),
                ],
                ElementsToReplace = [],
            });
        services.AddSingleton(lootGeneratorService);

        var recipePriceCalculator = Substitute.For<IRecipePriceCalculator>();
        recipePriceCalculator.GetItemPriceMap()
            .Returns(new Dictionary<string, RecipeOutputData>
            {
                { "WeaponRailgun1", new RecipeOutputData { Quanta = new Quanta(123) } },
                { "WeaponRailgun2", new RecipeOutputData { Quanta = new Quanta(123) } }
            });
        services.AddSingleton(recipePriceCalculator);

        var factionRepository = Substitute.For<IFactionRepository>();
        factionRepository.FindAsync(Arg.Any<FactionId>())
            .Returns(new FactionItem
            {
                Name = "Test Faction"
            });
        services.AddSingleton(factionRepository);

        var factionTerritoryRepository = Substitute.For<IFactionTerritoryRepository>();
        factionTerritoryRepository.GetAll()
            .Returns(new List<FactionTerritoryItem>
            {
                new()
            });
        services.AddSingleton(factionTerritoryRepository);

        var territoryContainerRepository = Substitute.For<ITerritoryContainerRepository>();
        territoryContainerRepository.GetAll(Arg.Any<DynamicEncounters.Features.Faction.Data.TerritoryId>())
            .Returns(new List<TerritoryContainerItem>
            {
                new()
            });
        services.AddSingleton(territoryContainerRepository);
        
        var bank = Substitute.For<IGameplayBank>();
        services.AddSingleton(bank);

        var sceneGraph = Substitute.For<IScenegraph>();
        sceneGraph.GetConstructCenterWorldPosition(Arg.Any<ConstructId>())
            .Returns(new Vec3());
        services.AddSingleton(sceneGraph);
        
        var provider = services.BuildServiceProvider();

        var generator = new ProceduralLootBasedMissionGeneratorService(provider);

        Assert.DoesNotThrowAsync(async () =>
        {
            var outcome = await generator.GenerateAsync(
                1,
                1,
                new DynamicEncounters.Features.Faction.Data.TerritoryId(Guid.NewGuid()),
                1234
            );

            Assert.That(outcome.Success);
        });
    }
}