using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Common.Services;
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

public class GiveTakeLootSkill(GiveTakeLootSkill.GiveTakeLootSkillItem skillItem) : BaseSkill(skillItem)
{
    public int CurrentIteration { get; set; }
    public ulong? OverrideConstructId { get; set; }
    public bool Finished { get; set; }
    public bool StateLoaded { get; set; }
    
    public override bool CanUse(BehaviorContext context)
    {
        return !context.Effects.IsEffectActive<CooldownEffect>() && !Finished && base.CanUse(context);
    }

    public override async Task Use(BehaviorContext context)
    {
        var provider = context.Provider;

        var stateService = provider.GetRequiredService<IConstructStateService>();
        
        if (!StateLoaded)
        {
            var stateOutcome = await stateService.Find(nameof(GiveTakeLootSkill), context.ConstructId);
            if (stateOutcome.Success)
            {
                var state = stateOutcome.StateItem!.Properties!.ToObject<State>();
                CurrentIteration = state.CurrentIteration;
            }

            StateLoaded = true;
        }
        
        if (CurrentIteration >= skillItem.MaxIterations)
        {
            Finished = true;
            if (skillItem.OnFinishedScript.Any())
            {
                var scriptAction = provider.GetScriptAction(skillItem.OnFinishedScript);
                await scriptAction.ExecuteAsync(new ScriptContext(
                    provider,
                    context.FactionId,
                    context.PlayerIds,
                    context.Sector,
                    context.TerritoryId)
                {
                    ConstructId = context.ConstructId
                });
            }
            return;
        }
        
        context.Effects.Activate<CooldownEffect>(TimeSpan.FromSeconds(skillItem.CooldownSeconds));
        CurrentIteration++;

        var constructId = OverrideConstructId ?? context.ConstructId;
        
        if (!skillItem.LootTags.Any()) return;

        var lootGeneratorService = provider.GetRequiredService<ILootGeneratorService>();
        var random = provider.GetRandomProvider().GetRandom();
        var lootBag = await lootGeneratorService.GenerateAsync(new LootGenerationArgs
        {
            Tags = skillItem.LootTags,
            Operator = TagOperator.AllTags,
            MaxBudget = skillItem.LootBudget,
            Seed = random.Next()
        });
        
        // TODO change to give take operation
        var itemSpawnerService = provider.GetRequiredService<IItemSpawnerService>();
        await itemSpawnerService.SpawnItems(new SpawnItemOnRandomContainersCommand(
            constructId,
            new ItemBagData
            {
                Name = string.Empty,
                MaxBudget = 1,
                Tags = [],
                Entries = lootBag.Entries
            }));

        await stateService.PersistState(new ConstructStateItem
        {
            Properties = JToken.FromObject(new State{CurrentIteration = CurrentIteration}),
            ConstructId = context.ConstructId,
            Type = nameof(GiveTakeLootSkill)
        });
        
        if (!skillItem.SendPlayerAlert) return;
            
        var constructService = provider.GetRequiredService<IConstructService>();
        var info = await constructService.GetConstructInfoAsync(constructId);
        var pilot = info.Info?.mutableData.pilot;
        
        if (!pilot.HasValue) return;

        var alertService = provider.GetRequiredService<IPlayerAlertService>();
        await alertService.SendInfoAlert(pilot.Value, skillItem.PlayerAlertMessage);
    }

    public static GiveTakeLootSkill Create(JToken jObj)
    {
        return new GiveTakeLootSkill(jObj.ToObject<GiveTakeLootSkillItem>());
    }

    public class CooldownEffect : IEffect;

    public class State
    {
        public required int CurrentIteration { get; set; }
    }

    public class GiveTakeLootSkillItem : SkillItem
    {
        [JsonProperty] public bool SendPlayerAlert { get; set; } = true;
        [JsonProperty] public string PlayerAlertMessage { get; set; } = "Items beamed to your cargo hold";
        [JsonProperty] public int MaxIterations { get; set; } = 10;
        [JsonProperty] public IEnumerable<ScriptActionItem> OnFinishedScript { get; set; } = [];
        [JsonProperty] public IEnumerable<string> LootTags { get; set; } = [];
        [JsonProperty] public double LootBudget { get; set; } = 25000;
    }
}