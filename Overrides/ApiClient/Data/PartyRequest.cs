namespace Mod.DynamicEncounters.Overrides.ApiClient.Data;

public class PartyRequest
{
    public ulong InstigatorPlayerId { get; set; }
    public ulong PlayerId { get; set; }
    public string Role { get; set; }
}