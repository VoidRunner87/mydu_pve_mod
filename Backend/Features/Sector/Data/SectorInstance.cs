using System;
using NQ;

namespace Mod.DynamicEncounters.Features.Sector.Data;

public class SectorInstance
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public Vec3 Sector { get; set; }
    public long FactionId { get; set; }
    public DateTime? PublishAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? ForceExpiresAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid TerritoryId { get; set; }
    public string OnLoadScript { get; set; }
    public string OnSectorEnterScript { get; set; }

    public SectorInstanceProperties Properties { get; set; }

    public bool IsForceExpired(DateTime dateTime)
        => ForceExpiresAt.HasValue && ForceExpiresAt.Value < dateTime;
}