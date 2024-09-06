using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Services;

public class PointGeneratorFactory : IPointGeneratorFactory
{
    public IPointGenerator Create(ScriptActionAreaItem item)
    {
        return item.Type switch
        {
            "sphere" => new SpherePointGenerator(item.Radius),
            _ => new SpherePointGenerator(100000)
        };
    }
}