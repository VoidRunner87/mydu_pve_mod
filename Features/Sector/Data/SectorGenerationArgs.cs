using System;
using System.Collections.Generic;
using Mod.DynamicEncounters.Features.Sector.Services;
using NQ;

namespace Mod.DynamicEncounters.Features.Sector.Data;

public class SectorGenerationArgs
{
    public double SectorGridSnap { get; set; } = SectorPoolManager.SectorGridSnap;
    public double MinRadius { get; set; } = 130 * 200 * 1000; // su to meters
    public double MaxRadius { get; set; } = 200 * 200 * 1000; // su to meters
    public Vec3 CenterPosition { get; set; } = new() { x = 13771471, y = 7435803, z = -128971 }; // default SZ bubble
    public int Quantity { get; set; } = 10;
    public TimeSpan ExpirationTimeSpan { get; set; } = TimeSpan.FromHours(3);

    public IEnumerable<SectorEncounterItem> Encounters { get; set; } = new List<SectorEncounterItem>();
}