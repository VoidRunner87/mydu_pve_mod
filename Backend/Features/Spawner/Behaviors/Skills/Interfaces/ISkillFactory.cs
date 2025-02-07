using System.Collections.Generic;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Interfaces;

public interface ISkillFactory
{
    ISkill Create(object item);
    IEnumerable<ISkill> CreateAll(IEnumerable<object> items);
}