using System;

namespace Mod.DynamicEncounters.Features.Sector.Data;

public class SectorEncounterItem
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string OnLoadScript { get; set; }
    public string OnSectorEnterScript { get; set; }
    public bool Active { get; set; }
    
    public EncounterProperties Properties { get; set; }
}