using System;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Services;

public class SpherePointGenerator(float radius) : IPointGenerator
{
    public Vec3 NextPoint(Random random) => random.RandomDirectionVec3() * radius;
}