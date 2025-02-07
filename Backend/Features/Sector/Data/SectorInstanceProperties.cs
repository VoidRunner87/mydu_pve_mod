using System.Collections.Generic;

namespace Mod.DynamicEncounters.Features.Sector.Data;

public class SectorInstanceProperties
{
    public IEnumerable<string> Tags { get; set; } = [];
    
    /// <summary>
    /// Indicates that a sector instance should have a visible activate marker on the POI list
    /// </summary>
    public bool HasActiveMarker { get; set; }
}