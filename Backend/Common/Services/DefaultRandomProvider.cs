using System;
using Mod.DynamicEncounters.Common.Interfaces;

namespace Mod.DynamicEncounters.Common.Services;

public class DefaultRandomProvider : IRandomProvider
{
    private readonly Random _random = new();
    public Random GetRandom() => _random;
}