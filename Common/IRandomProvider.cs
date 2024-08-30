using System;

namespace Mod.DynamicEncounters.Common;

public interface IRandomProvider
{
    Random GetRandom();
}