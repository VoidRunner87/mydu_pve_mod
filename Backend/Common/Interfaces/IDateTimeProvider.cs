using System;

namespace Mod.DynamicEncounters.Common.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow();
}