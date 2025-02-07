namespace Mod.DynamicEncounters.Overrides.ApiClient.Services;

public class SetWarpEndCooldownRequest
{
    public ulong ConstructId { get; set; }
    public string ElementTypeName { get; set; } = string.Empty;
}