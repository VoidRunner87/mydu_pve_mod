using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Scripts.Actions;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Data;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Data;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Services;

public class WaveScriptSkill(WaveScriptSkill.WaveScriptSkillItem skillItem) : BaseSkill(skillItem)
{
    public int CurrentCycle { get; set; }
    public ConcurrentBag<ulong> SpawnedConstructs { get; set; } = [];
    public bool StateLoaded { get; set; }

    public override bool CanUse(BehaviorContext context)
    {
        return CurrentCycle < skillItem.CycleCount &&
               !context.Effects.IsEffectActive<CooldownEffect>();
    }

    public override async Task Use(BehaviorContext context)
    {
        var provider = context.Provider;
        var stateService = provider.GetRequiredService<IConstructStateService>();

        await LoadState(context, stateService);

        context.Effects.Activate<CooldownEffect>(TimeSpan.FromSeconds(skillItem.CooldownSeconds));
        CurrentCycle++;

        var actionFactory = provider.GetRequiredService<IScriptActionFactory>();
        var scriptAction = actionFactory.Create(skillItem.Script);

        var scriptContext = new ScriptContext(
            provider,
            context.FactionId,
            context.PlayerIds,
            context.Sector,
            context.TerritoryId)
        {
            ConstructId = context.ConstructId,
        };
        scriptContext.OnEvent += (_, args) => HandleEvent((dynamic)args);

        await scriptAction.ExecuteAsync(scriptContext);

        await stateService.PersistState(new ConstructStateItem
        {
            ConstructId = context.ConstructId,
            Type = nameof(WaveScriptSkill),
            Properties = JToken.FromObject(new WaveScriptSkillState
            {
                CurrentCycle = CurrentCycle,
                SpawnedConstructs = SpawnedConstructs.ToList()
            })
        });
    }

    private async Task LoadState(BehaviorContext context, IConstructStateService stateService)
    {
        if (StateLoaded) return;

        var outcome = await stateService.Find(nameof(WaveScriptSkill), context.ConstructId);
        if (outcome.Success)
        {
            var state = outcome.StateItem!.Properties!.ToObject<WaveScriptSkillState>();

            CurrentCycle = state.CurrentCycle;
            SpawnedConstructs = new ConcurrentBag<ulong>(state.SpawnedConstructs);
        }

        StateLoaded = true;
    }

    private void HandleEvent(SpawnScriptAction.ConstructSpawnedEvent @event)
    {
        SpawnedConstructs.Add(@event.GetData<ulong>());
    }

    private void HandleEvent(ScriptContextEventArgs _)
    {
        // no op
    }

    public static WaveScriptSkill Create(JObject item)
    {
        return new WaveScriptSkill(item.ToObject<WaveScriptSkillItem>());
    }

    public class CooldownEffect : IEffect;

    public class WaveScriptSkillItem : SkillItem
    {
        [JsonProperty] public int CycleCount { get; set; } = 3;
        [JsonProperty] public IEnumerable<ScriptActionItem> Script { get; set; } = [];
    }

    public class WaveScriptSkillState
    {
        [JsonProperty] public int CurrentCycle { get; set; }
        [JsonProperty] public IList<ulong> SpawnedConstructs { get; set; } = [];
    }
}