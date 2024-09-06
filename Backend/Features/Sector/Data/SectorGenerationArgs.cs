using System.Collections.Generic;
using Mod.DynamicEncounters.Features.Sector.Services;

namespace Mod.DynamicEncounters.Features.Sector.Data;

public class SectorGenerationArgs
{
    public double SectorGridSnap { get; set; } = SectorPoolManager.SectorGridSnap;
    public int Quantity { get; set; } = 10;

    /// <summary>
    /// Minimum Gap of SectorSize between Sectors 
    /// </summary>
    public int SectorMinimumGap = 3;

    public IEnumerable<SectorEncounterItem> Encounters { get; set; } = new List<SectorEncounterItem>();
}