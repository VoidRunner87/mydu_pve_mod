using System;
using System.Collections.Generic;
using System.Linq;
using NQ;

namespace Mod.DynamicEncounters.Helpers;

public static class RandomHelpers
{
    public static Vec3 RandomDirectionVec3(this Random random)
    {
        return new Vec3
        {
            x = random.Next(-1, 1),
            y = random.Next(-1, 1),
            z = random.Next(-1, 1),
        }.Normalized();
    }

    public static T PickOneAtRandom<T>(this Random random, IEnumerable<T> items)
    {
        var itemsList = items.ToList();
        
        return itemsList[random.Next(itemsList.Count)];
    }
}