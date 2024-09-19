namespace Mod.DynamicEncounters.Features.Faction.Data;

public class FactionItem
{
    public long Id { get; set; }
    public string Tag { get; set; } = "";
    public string Name { get; set; } = "";
    public ulong? OrganizationId { get; set; }
    public ulong PlayerId { get; set; } = 4;
    public FactionProperties Properties { get; set; } = new();
    
    public class FactionProperties
    {
        public int SectorPoolCount { get; set; } = 10;
    }
}