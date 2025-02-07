using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Extensions;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Data;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Data;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Services;

public class WaypointSkill(WaypointSkill.WaypointSkillItem skillItem) : BaseSkill(skillItem)
{
    public bool WaypointInitialized { get; set; }
    public Queue<WaypointItem> WaypointQueue { get; set; } = new(skillItem.Waypoints);

    public override async Task Use(BehaviorContext context)
    {
        if (!context.Position.HasValue) return;

        if (!WaypointInitialized)
        {
            await InitializeWaypointQueue(context);

            WaypointInitialized = true;
        }

        if (!context.Contacts.IsEmpty && skillItem.InterruptWaypointNavigationOnPlayerContact)
        {
            context.SetOverrideTargetMovePosition(null);
            return;
        }

        if (WaypointQueue.Count == 0)
        {
            if (skillItem.ResetWaypointOnArrival)
            {
                WaypointQueue = new Queue<WaypointItem>(skillItem.Waypoints);
            }

            return;
        }

        var nextWaypoint = WaypointQueue.Peek();
        if (context.Position.Value.Dist(nextWaypoint.Position) < skillItem.WaypointArrivalDistance)
        {
            WaypointQueue.Dequeue();
            await OnArrivedAtWaypoint(context);
            await PersistWaypointQueue(context);

            var arrivedAtFinalDestination = WaypointQueue.Count == 0;
            if (arrivedAtFinalDestination)
            {
                var scriptAction = context.Provider.GetScriptAction(skillItem.ArrivedAtFinalDestinationScript);

                await scriptAction.ExecuteAsync(new ScriptContext(
                    context.Provider,
                    context.FactionId,
                    context.PlayerIds,
                    context.Sector,
                    context.TerritoryId)
                {
                    ConstructId = context.ConstructId
                });
            }
        }

        context.SetOverrideTargetMovePosition(nextWaypoint.Position);
    }

    public virtual async Task PersistWaypointQueue(BehaviorContext context)
    {
        var stateService = context.Provider.GetRequiredService<IConstructStateService>();
        await stateService.PersistState(new ConstructStateItem
        {
            Properties = JToken.FromObject(WaypointQueue.ToList()),
            ConstructId = context.ConstructId,
            Type = nameof(WaypointSkill)
        });
    }

    public virtual Task OnArrivedAtWaypoint(BehaviorContext context)
    {
        return Task.CompletedTask;
    }

    public virtual async Task InitializeWaypointQueue(BehaviorContext context)
    {
        var stateService = context.Provider.GetRequiredService<IConstructStateService>();
        var outcome = await stateService.Find(nameof(WaypointSkill), context.ConstructId);
        if (outcome.Success)
        {
            var waypointList = outcome.StateItem!.Properties!.ToObject<IEnumerable<WaypointItem>>();
            WaypointQueue = new Queue<WaypointItem>(waypointList);
        }
    }

    public static WaypointSkill Create(JObject item)
    {
        return new WaypointSkill(item.ToObject<WaypointSkillItem>());
    }

    public class WaypointSkillItem : SkillItem
    {
        [JsonProperty] public IEnumerable<WaypointItem> Waypoints { get; set; } = [];
        [JsonProperty] public double WaypointArrivalDistance { get; set; } = 50000;
        [JsonProperty] public IEnumerable<ScriptActionItem> ArrivedAtFinalDestinationScript { get; set; } = [];
        [JsonProperty] public bool InterruptWaypointNavigationOnPlayerContact { get; set; }
        [JsonProperty] public bool ResetWaypointOnArrival { get; set; }
    }
}