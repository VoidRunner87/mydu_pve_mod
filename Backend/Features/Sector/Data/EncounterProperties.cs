using System;
using NQ;

namespace Mod.DynamicEncounters.Features.Sector.Data;

public class EncounterProperties
{
    public double MinRadius { get; set; } = 130 * 200 * 1000; // su to meters
    public double MaxRadius { get; set; } = 200 * 200 * 1000; // su to meters
    public Vec3 CenterPosition { get; set; } = new() { x = 13771471, y = 7435803, z = -128971 }; // default SZ bubble
    public TimeSpan ExpirationTimeSpan { get; set; } = TimeSpan.FromHours(3);
    public TimeSpan? ForcedExpirationTimeSpan { get; set; }

    /// <summary>
    /// Indicates that a sector instance should have a visible activate marker on the POI list
    /// </summary>
    public bool HasActiveMarker { get; set; }
}