using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Data;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Services;

public class RunScriptSkill(RunScriptSkill.RunScriptSkillItem skillItem) : BaseSkill(skillItem)
{
    public override bool CanUse(BehaviorContext context)
    {
        return !context.Effects.IsEffectActive<CooldownEffect>() && base.CanUse(context);
    }

    public override Task Use(BehaviorContext context)
    {
        context.Effects.Activate<CooldownEffect>(TimeSpan.FromSeconds(skillItem.CooldownSeconds));

        var scriptActionFactory = context.Provider.GetRequiredService<IScriptActionFactory>();
        var scriptAction = scriptActionFactory.Create(skillItem.Script);
        scriptAction.ExecuteAsync(new ScriptContext(
            context.Provider,
            context.FactionId,
            context.PlayerIds,
            context.Sector,
            context.TerritoryId)
        {
            ConstructId = context.ConstructId
        });
        
        return Task.CompletedTask;
    }
    
    public static RunScriptSkill Create(JObject item)
    {
        return new RunScriptSkill(item.ToObject<RunScriptSkillItem>());
    }
    
    public class CooldownEffect : IEffect;

    public class RunScriptSkillItem : SkillItem
    {
        [JsonProperty] public ScriptActionItem Script { get; set; } = new();
    }
}