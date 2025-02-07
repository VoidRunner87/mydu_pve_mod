using System;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using NQ;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Services;

public class NullPointGenerator : IPointGenerator
{
    public Vec3 NextPoint(Random random)
    {
        return new Vec3();
    }
}