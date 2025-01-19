using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Loot.Data;
using Mod.DynamicEncounters.Features.Loot.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Extensions;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Data;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Data;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Mod.DynamicEncounters.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Services;

public class FacilityStrikeScenarioSkill(
    FacilityStrikeScenarioSkill.FacilityStrikeScenarioSkillItem skillItem,
    ProduceLootWhenSafeSkill produceLootSkill
) : BaseSkill(skillItem)
{
    public FacilityStrikeState? State { get; set; }
    public bool ReadyForNextWave { get; set; }

    public override bool CanUse(BehaviorContext context)
    {
        return (State is null or { Finished: false } || !produceLootSkill.Finished) && base.CanUse(context);
    }

    public override bool ShouldUse(BehaviorContext context) =>
        !context.Effects.IsEffectActive<UseCooldownEffect>() && base.ShouldUse(context);

    public override async Task Use(BehaviorContext context)
    {
        var provider = context.Provider;
        var position = context.Position ?? context.Sector;

        context.Effects.Activate<UseCooldownEffect>(TimeSpan.FromSeconds(skillItem.CooldownSeconds));

        if (!skillItem.Waves.Any()) return;

        var constructStateService = context.Provider.GetRequiredService<IConstructStateService>();

        await LoadState(context, constructStateService);
        if (produceLootSkill.CanUse(context) && produceLootSkill.ShouldUse(context))
        {
            await produceLootSkill.Use(context);
        }

        if (State == null)
        {
            var logger = provider.CreateLogger<FacilityStrikeScenarioSkill>();
            logger.LogError("Invalid State");
            return;
        }

        var waves = skillItem.Waves.ToList();
        var previousWaveIndex = State.CurrentWaveIndex - 1;
        if (waves.Count < State!.CurrentWaveIndex + 1)
        {
            State.Finished = true;
            if (produceLootSkill.Finished)
            {
                if (previousWaveIndex >= 0)
                {
                    await SpawnWaveRewardItems(context, context.Provider, waves[previousWaveIndex]);
                }

                await FinishScenario(context);
            }

            await PersistState(context, constructStateService);

            return;
        }

        if (context.Effects.IsEffectActive<NextWaveCooldownEffect>()) return;

        var wave = waves[State!.CurrentWaveIndex];

        if (skillItem.NewWaveOnlyWhenClear)
        {
            var areaScanService = provider.GetRequiredService<IAreaScanService>();
            var contacts = (await areaScanService.ScanForNpcConstructs(position, skillItem.AreScanRange))
                .Select(x => x.ConstructId)
                .ToHashSet();

            contacts.Remove(context.ConstructId);

            if (contacts.Count != 0) return;

            if (!ReadyForNextWave)
            {
                context.Effects.Activate<NextWaveCooldownEffect>(TimeSpan.FromSeconds(wave.Cooldown));
                ReadyForNextWave = true;

                var beforeScript = context.Provider.GetScriptAction(wave.BeforeScript);
                await beforeScript.ExecuteAsync(context.GetScriptContext());
                
                return;
            }
        }

        context.Effects.Activate<NextWaveCooldownEffect>(TimeSpan.FromSeconds(wave.Cooldown));

        if (previousWaveIndex >= 0)
        {
            var previousWave = waves[previousWaveIndex];
            await SpawnWaveRewardItems(context, provider, previousWave);
        }

        State.CurrentWaveIndex++;
        ReadyForNextWave = false;

        await PersistState(context, constructStateService);

        var scriptAction = context.Provider.GetScriptAction(wave.Script);
        await scriptAction.ExecuteAsync(context.GetScriptContext());
    }

    private static async Task SpawnWaveRewardItems(
        BehaviorContext context,
        IServiceProvider provider,
        FacilityStrikeScenarioSkillItem.WaveItem wave)
    {
        var lootGeneratorService = provider.GetRequiredService<ILootGeneratorService>();
        var random = provider.GetRandomProvider().GetRandom();
        var lootBag = await lootGeneratorService.GenerateAsync(new LootGenerationArgs
        {
            Tags = wave.RewardLootTags,
            Operator = TagOperator.AllTags,
            MaxBudget = wave.RewardLootBudget,
            Seed = random.Next()
        });

        var itemSpawnerService = provider.GetRequiredService<IItemSpawnerService>();
        await itemSpawnerService.SpawnItemsForPlayersAround(new SpawnItemOnRandomContainersAroundAreaCommand
        {
            InstigatorConstructId = context.ConstructId,
            Position = context.Position ?? context.Sector,
            Radius = DistanceHelpers.OneSuInMeters * 5,
            ItemBag = new ItemBagData
            {
                Name = string.Empty,
                MaxBudget = 1,
                Tags = [],
                Entries = lootBag.Entries
            }
        });
    }

    private async Task PersistState(BehaviorContext context, IConstructStateService constructStateService)
    {
        if (State == null) return;

        await constructStateService.PersistState(new ConstructStateItem
        {
            Properties = JToken.FromObject(State),
            Type = nameof(FacilityStrikeState),
            ConstructId = context.ConstructId
        });
    }

    private async Task FinishScenario(BehaviorContext context)
    {
        await context.Provider.GetScriptAction(skillItem.OnFinishedScript)
            .ExecuteAsync(new ScriptContext(
                context.Provider,
                context.FactionId,
                context.PlayerIds,
                context.Sector,
                context.TerritoryId).WithConstructId(context.ConstructId));
    }

    private async Task LoadState(BehaviorContext context, IConstructStateService constructStateService)
    {
        if (State == null)
        {
            var outcome = await constructStateService.Find(nameof(FacilityStrikeState), context.ConstructId);

            State = new FacilityStrikeState();

            if (outcome.Success)
            {
                State = outcome.StateItem?.Properties?.ToObject<FacilityStrikeState>();
            }
        }
    }

    public static FacilityStrikeScenarioSkill Create(JToken jObj)
    {
        var facilityStrikeSkillItem = jObj.ToObject<FacilityStrikeScenarioSkillItem>();

        return new FacilityStrikeScenarioSkill(
            facilityStrikeSkillItem,
            ProduceLootWhenSafeSkill.Create(jObj[nameof(FacilityStrikeScenarioSkillItem.Production)])
        );
    }

    public class FacilityStrikeState
    {
        [JsonProperty] public bool Finished { get; set; }
        [JsonProperty] public int CurrentWaveIndex { get; set; }
    }

    public class NextWaveCooldownEffect : IEffect;

    public class UseCooldownEffect : IEffect;

    public class FacilityStrikeScenarioSkillItem : SkillItem
    {
        [JsonProperty] public ProduceLootWhenSafeSkill.ProduceLootWhenSafe? Production { get; set; }
        [JsonProperty] public IEnumerable<WaveItem> Waves { get; set; } = [];
        [JsonProperty] public IEnumerable<ScriptActionItem> OnFinishedScript { get; set; } = [];
        [JsonProperty] public double AreScanRange { get; set; } = DistanceHelpers.OneSuInMeters * 3D;
        [JsonProperty] public bool NewWaveOnlyWhenClear { get; set; } = true;

        public class WaveItem
        {
            [JsonProperty] public IEnumerable<ScriptActionItem> Script { get; set; } = [];
            [JsonProperty] public IEnumerable<ScriptActionItem> BeforeScript { get; set; } = [];
            [JsonProperty] public double Cooldown { get; set; } = 60D;
            [JsonProperty] public IEnumerable<string> RewardLootTags { get; set; } = [];
            [JsonProperty] public double RewardLootBudget { get; set; } = 25000;
        }
    }
}