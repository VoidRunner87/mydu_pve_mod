using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Data;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Services;

public abstract class BaseSkill(SkillItem skillItem) : ISkill
{
    public string Name { get; set; } = skillItem.Name;
    public bool Active { get; set; } = skillItem.Active;
    public virtual bool CanUse(BehaviorContext context) => Active && context.IsAlive;

    public virtual bool ShouldUse(BehaviorContext context) => Active;

    public abstract Task Use(BehaviorContext context);
}