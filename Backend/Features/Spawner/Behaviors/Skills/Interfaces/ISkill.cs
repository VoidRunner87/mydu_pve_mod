using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Interfaces;

public interface ISkill
{
    bool CanUse(BehaviorContext context);
    bool ShouldUse(BehaviorContext context);
    Task Use(BehaviorContext context);
}