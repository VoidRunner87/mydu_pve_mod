using System;
using Mod.DynamicEncounters.Features.Sector.Repository;
using NQ;

namespace Mod.DynamicEncounters.Features.Sector.Data;

public class EncounterProperties
{
    public string[] Tags { get; set; } = [SectorEncounterTags.Pooled];
    public double MinRadius { get; set; } = 130 * 200 * 1000; // su to meters
    public double MaxRadius { get; set; } = 200 * 200 * 1000; // su to meters
    public Vec3 CenterPosition { get; set; } = new() { x = 13771471, y = 7435803, z = -128971 }; // default SZ bubble
    public TimeSpan ExpirationTimeSpan { get; set; } = TimeSpan.FromHours(3);
}