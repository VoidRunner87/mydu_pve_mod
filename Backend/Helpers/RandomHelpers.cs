using System;
using System.Collections.Generic;
using System.Linq;
using NQ;

namespace Mod.DynamicEncounters.Helpers;

public static class RandomHelpers
{
    public static Vec3 RandomDirectionVec3(this Random random)
    {
        var theta = random.NextDouble() * 2 * Math.PI;
        var phi = Math.Acos(2 * random.NextDouble() - 1);

        // Convert spherical coordinates to Cartesian coordinates
        var x = Math.Sin(phi) * Math.Cos(theta);
        var y = Math.Sin(phi) * Math.Sin(theta);
        var z = Math.Cos(phi);

        return new Vec3 { x = x, y = y, z = z };
    }

    public static T PickOneAtRandom<T>(this Random random, IEnumerable<T> items)
    {
        var itemsList = items.ToList();

        return itemsList[random.Next(itemsList.Count)];
    }
}