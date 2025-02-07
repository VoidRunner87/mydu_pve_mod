using Mod.DynamicEncounters.Common.Interfaces;

namespace Mod.DynamicEncounters.Features.Sector.Data;

public class SectorActivationOutcome : IOutcome
{
    public bool Success { get; init; }
    public string Message { get; init; }

    public static SectorActivationOutcome Activated() => new() { Success = true, Message = "" };
    public static SectorActivationOutcome Failed(string message) => new() { Success = false, Message = message };
}