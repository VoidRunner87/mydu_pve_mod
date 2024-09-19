namespace Mod.DynamicEncounters.Features.Faction.Data;

public class FactionItem
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public ulong OrganizationId { get; set; } = 0;
    public FactionProperties Properties { get; set; } = new();
    
    public class FactionProperties
    {
        public int SectorPoolCount { get; set; } = 10;
    }
}