using Mod.DynamicEncounters.Common.Interfaces;
using NQ;

namespace Mod.DynamicEncounters.Features.Common.Interfaces;

public class ConstructInfoOutcome(bool constructExists, ConstructInfo? info) : IOutcome
{
    public bool ConstructExists { get; } = constructExists;
    public ConstructInfo? Info { get; } = info;

    public static ConstructInfoOutcome DoesNotExist() => new ConstructInfoOutcome(false, null);
}