using System;
using Mod.DynamicEncounters.Common.Interfaces;

namespace Mod.DynamicEncounters.Common.Services;

public class DefaultDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow() => DateTime.UtcNow;
}