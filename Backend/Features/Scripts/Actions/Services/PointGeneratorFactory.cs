using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Services;

public class PointGeneratorFactory : IPointGeneratorFactory
{
    public IPointGenerator Create(ScriptActionAreaItem item)
    {
        return item.Type switch
        {
            "sphere" => new SpherePointGenerator(item.MinRadius, item.Radius),
            "ring" => new RingPointGenerator(
                item.MinRadius,
                item.Radius,
                item.Height,
                item.Rotation.ToQuaternion()
            ),
            _ => new NullPointGenerator()
        };
    }
}