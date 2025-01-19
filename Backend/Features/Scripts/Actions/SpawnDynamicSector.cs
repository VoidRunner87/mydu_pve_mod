using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Services;
using Mod.DynamicEncounters.Features.Sector.Data;
using Mod.DynamicEncounters.Features.Sector.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Services;
using Mod.DynamicEncounters.Features.Spawner.Extensions;
using Mod.DynamicEncounters.Helpers;
using Newtonsoft.Json;
using NQ;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

[ScriptActionName(ActionName)]
public class SpawnDynamicSector(ScriptActionItem actionItem) : IScriptAction
{
    public const string ActionName = "spawn-sector";
    public string Name => ActionName;
    public string GetKey() => Name;

    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var provider = context.ServiceProvider;
        var logger = provider.CreateLogger<SpawnDynamicSector>();
        var sectorInstanceRepository = provider.GetRequiredService<ISectorInstanceRepository>();

        actionItem.Properties.Merge(context.Properties.ToDictionary());
        var props = actionItem.GetProperties<Properties>();

        var territoryId = context.TerritoryId ?? actionItem.TerritoryId;
        
        if (!territoryId.HasValue)
        {
            logger.LogError("Missing Territory on Sector Instance Spawn");
            
            return ScriptActionResult.Failed();
        }

        await sectorInstanceRepository.AddAsync(new SectorInstance
        {
            Name = props.Name,
            Sector = (props.Position ?? context.Sector).GridSnap(props.SectorSize),
            FactionId = context.FactionId ?? 1,
            PublishAt = props.PublishTimeSpan.HasValue ? DateTime.UtcNow + props.PublishTimeSpan : null,
            ExpiresAt = DateTime.UtcNow + props.ExpirationTimeSpan,
            ForceExpiresAt = DateTime.UtcNow + props.ForceExpirationTimeSpan,
            Id = Guid.NewGuid(),
            OnLoadScript = props.OnLoadScript,
            OnSectorEnterScript = props.OnSectorEnterScript,
            TerritoryId = territoryId.Value,
            Properties = new SectorInstanceProperties
            {
                Tags = props.Tags,
                HasActiveMarker = props.HasActiveMarker
            }
        });
        
        return ScriptActionResult.Successful();
    }

    public class Properties
    {
        [JsonProperty] public string OnLoadScript { get; set; } = string.Empty;
        [JsonProperty] public string OnSectorEnterScript { get; set; } = string.Empty;
        [JsonProperty] public Vec3? Position { get; set; }
        [JsonProperty] public TimeSpan? PublishTimeSpan { get; set; }
        [JsonProperty] public TimeSpan ExpirationTimeSpan { get; set; }
        [JsonProperty] public TimeSpan ForceExpirationTimeSpan { get; set; }
        [JsonProperty] public string[] Tags { get; set; } = [];
        [JsonProperty] public bool HasActiveMarker { get; set; }
        [JsonProperty] public string Name { get; set; } = string.Empty;
        [JsonProperty] public double SectorSize { get; set; } = SectorPoolManager.SectorGridSnap;
    }
}