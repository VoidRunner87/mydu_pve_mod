using System;

namespace Mod.DynamicEncounters.Features.Spawner.Data;

public class DamageDealtData
{
    public ulong ConstructId { get; set; }
    public ulong PlayerId { get; set; }
    public required double Damage { get; set; }
    public required string Type { get; set; }
    public required DateTime DateTime { get; set; }
}