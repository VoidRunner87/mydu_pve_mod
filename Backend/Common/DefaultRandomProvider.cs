using System;

namespace Mod.DynamicEncounters.Common;

public class DefaultRandomProvider : IRandomProvider
{
    private readonly Random _random = new();
    public Random GetRandom() => _random;
}