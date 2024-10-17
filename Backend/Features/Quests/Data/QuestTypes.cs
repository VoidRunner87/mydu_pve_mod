using System.Collections.Generic;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public static class QuestTypes
{
    public const string Exploration = "exploration";
    public const string Combat = "combat";
    public const string Transport = "transport";

    public static IEnumerable<string> All()
        => new[]
        {
            Transport
        };
}