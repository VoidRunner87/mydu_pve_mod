using System;
using NQ;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

public interface IPointGenerator
{
    Vec3 NextPoint(Random random);
}