using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

public interface IPointGeneratorFactory
{
    IPointGenerator Create(ScriptActionAreaItem item);
}