using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Data;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Effects.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Services;

public class AsteroidRoamSkill(AsteroidRoamSkill.AsteroidRoamSkillItem skillItem) : WaypointSkill(skillItem)
{
    public override async Task InitializeWaypointQueue(BehaviorContext context)
    {
        var provider = context.Provider;
        var constructRepository = provider.GetRequiredService<IConstructRepository>();
        var travelRouteService = provider.GetRequiredService<ITravelRouteService>();

        var asteroids = await constructRepository.FindAsteroids();
        var route = travelRouteService.Solve(new WaypointItem
            {
                Position = context.Position!.Value
            },
            asteroids.Select(a => new WaypointItem
            {
                Name = a.Name,
                Position = a.Position
            }));

        WaypointQueue = new Queue<WaypointItem>(route.Take(10));
    }

    public override async Task Use(BehaviorContext context)
    {
        await base.Use(context);

        if (!context.Effects.IsEffectActive<RerouteCooldown>())
        {
            context.Effects.Activate<RerouteCooldown>(TimeSpan.FromSeconds(skillItem.RerouteCooldownSeconds));
            WaypointInitialized = false;
        }
    }
    
    public new static AsteroidRoamSkill Create(JObject item)
    {
        return new AsteroidRoamSkill(item.ToObject<AsteroidRoamSkillItem>());
    }

    public class RerouteCooldown : IEffect;

    public class AsteroidRoamSkillItem : WaypointSkillItem
    {
        [JsonProperty] public double RerouteCooldownSeconds { get; set; } = 30 * 60;
    }
}