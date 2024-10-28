using System;

namespace Mod.DynamicEncounters.Features.Quests.Services;

public static class MissionProceduralGenerationConfig
{
    public static readonly TimeSpan TimeFactor = TimeSpan.FromMinutes(50);
    public const float TransportQuantaMultiplier = 1.6f;
    public const float ReverseTransportMultiplier = 2.7f;
}