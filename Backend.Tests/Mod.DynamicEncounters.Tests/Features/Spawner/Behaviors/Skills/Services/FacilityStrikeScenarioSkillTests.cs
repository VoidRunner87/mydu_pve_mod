using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Common.Interfaces;
using Mod.DynamicEncounters.Common.Services;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Loot.Data;
using Mod.DynamicEncounters.Features.Loot.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Data;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Services;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Tests.Features.Common.Services;
using Mod.DynamicEncounters.Tests.Features.Spawner.Behaviors.Services;
using Newtonsoft.Json.Linq;
using NQ;
using NSubstitute;

namespace Mod.DynamicEncounters.Tests.Features.Spawner.Behaviors.Skills.Services;

[TestFixture]
public class FacilityStrikeScenarioSkillTests
{
    [Test]
    public void Should_Execute_Wave()
    {
        const ulong constructId = 1;

        ServiceCollection services = [];

        var dateTimeProvider = new DateTimeProviderStub();
        services.AddSingleton<IDateTimeProvider>(dateTimeProvider);

        services.AddSingleton<IRandomProvider>(new DefaultRandomProvider());

        var scriptActionFactory = Substitute.For<IScriptActionFactory>();
        scriptActionFactory.Create(Arg.Any<IEnumerable<ScriptActionItem>>())
            .Returns(new NullScriptAction());
        services.AddSingleton(scriptActionFactory);

        var constructStateService = Substitute.For<IConstructStateService>();
        constructStateService.WithState(nameof(FacilityStrikeScenarioSkill.FacilityStrikeState),
            constructId,
            ConstructStateOutcome.Retrieved(new ConstructStateItem
            {
                Properties = JToken.FromObject(new object())
            }));
        services.AddSingleton(constructStateService);

        var areaScanService = Substitute.For<IAreaScanService>();
        services.AddSingleton(areaScanService);

        var spawnerItemService = Substitute.For<IItemSpawnerService>();
        services.AddSingleton(spawnerItemService);

        var lootService = Substitute.For<ILootGeneratorService>();
        lootService.GenerateAsync(Arg.Any<LootGenerationArgs>())
            .Returns(new ItemBagData
            {
                Name = "loot",
                MaxBudget = 25000,
                Tags = ["loot-test"]
            });
        services.AddSingleton(lootService);

        services.AddLogging();

        var provider = services.BuildServiceProvider();

        var prefab = new Prefab(new PrefabItem());

        var scenario = new FacilityStrikeScenarioSkill(
            new FacilityStrikeScenarioSkill.FacilityStrikeScenarioSkillItem
            {
                Name = "test",
                CooldownSeconds = 5D,
                Waves =
                [
                    new FacilityStrikeScenarioSkill.FacilityStrikeScenarioSkillItem.WaveItem
                    {
                        Script = [],
                        Cooldown = 60D,
                        RewardLootTags = ["loot-test"],
                        RewardLootBudget = 25000
                    },
                    new FacilityStrikeScenarioSkill.FacilityStrikeScenarioSkillItem.WaveItem
                    {
                        Script = [],
                        Cooldown = 60D,
                        RewardLootTags = ["loot-test"],
                        RewardLootBudget = 25000
                    },
                    new FacilityStrikeScenarioSkill.FacilityStrikeScenarioSkillItem.WaveItem
                    {
                        Script = [],
                        Cooldown = 60D,
                        RewardLootTags = ["loot-test"],
                        RewardLootBudget = 25000
                    },
                    new FacilityStrikeScenarioSkill.FacilityStrikeScenarioSkillItem.WaveItem
                    {
                        Script = [],
                        Cooldown = 60D,
                        RewardLootTags = ["loot-test"],
                        RewardLootBudget = 25000
                    },
                ]
            },
            new ProduceLootWhenSafeSkill(new ProduceLootWhenSafeSkill.ProduceLootWhenSafe
            {
                Name = "test",
                CooldownSeconds = 5D,
            }){Finished = true});

        var context = new BehaviorContext(constructId, 1, null, new Vec3(), provider, prefab);

        Assert.That(scenario.CanUse(context));
        Assert.DoesNotThrowAsync(() => scenario.Use(context));
        lootService.DidNotReceive().GenerateAsync(Arg.Any<LootGenerationArgs>());
        dateTimeProvider.AddTime(TimeSpan.FromMinutes(2));
        
        Assert.DoesNotThrowAsync(() => scenario.Use(context));
        lootService.DidNotReceive().GenerateAsync(Arg.Any<LootGenerationArgs>());
        dateTimeProvider.AddTime(TimeSpan.FromMinutes(2));
        
        Assert.DoesNotThrowAsync(() => scenario.Use(context));
        lootService.DidNotReceive().GenerateAsync(Arg.Any<LootGenerationArgs>());
        dateTimeProvider.AddTime(TimeSpan.FromMinutes(2));
        
        Assert.DoesNotThrowAsync(() => scenario.Use(context));
        lootService.Received(1).GenerateAsync(Arg.Any<LootGenerationArgs>());
        dateTimeProvider.AddTime(TimeSpan.FromMinutes(2));
        
        Assert.DoesNotThrowAsync(() => scenario.Use(context));
        dateTimeProvider.AddTime(TimeSpan.FromMinutes(2));
        
        Assert.DoesNotThrowAsync(() => scenario.Use(context));
        dateTimeProvider.AddTime(TimeSpan.FromMinutes(2));
        
        Assert.DoesNotThrowAsync(() => scenario.Use(context));
        dateTimeProvider.AddTime(TimeSpan.FromMinutes(2));
        
        Assert.DoesNotThrowAsync(() => scenario.Use(context));
        dateTimeProvider.AddTime(TimeSpan.FromMinutes(2));
        
        Assert.DoesNotThrowAsync(() => scenario.Use(context));
        Assert.That(scenario.State, Is.Not.Null);
        Assert.That(scenario.State.Finished, Is.True);
        Assert.That(scenario.CanUse(context), Is.False);
    }
}