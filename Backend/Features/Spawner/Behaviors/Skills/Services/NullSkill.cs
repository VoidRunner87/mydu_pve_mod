using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Services;

public class NullSkill : ISkill
{
    public bool CanUse(BehaviorContext context)
    {
        return false;
    }

    public bool ShouldUse(BehaviorContext context)
    {
        return false;
    }

    public Task Use(BehaviorContext context)
    {
        return Task.CompletedTask;
    }
}