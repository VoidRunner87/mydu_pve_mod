namespace Mod.DynamicEncounters.Overrides.ApiClient.Services;

public class RegisterDamageRequest
{
    public ulong ConstructId { get; set; }
    public ulong InstigatorConstructId { get; set; }
    public ulong PlayerId { get; set; }
    public double Damage { get; set; }
    public string Type { get; set; } = "shield-hit";
}