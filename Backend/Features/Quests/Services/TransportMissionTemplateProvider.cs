using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Loot.Data;
using Mod.DynamicEncounters.Features.Quests.Data;
using Mod.DynamicEncounters.Features.Quests.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ;

namespace Mod.DynamicEncounters.Features.Quests.Services;

/// <summary>
/// TODO to the Database and maybe add Faction Specific stuff
/// </summary>
public class TransportMissionTemplateProvider : ITransportMissionTemplateProvider
{
    public async Task<TransportMissionTemplate> GetMissionTemplate(int seed)
    {
        await Task.Yield();

        var random = new Random(seed);
        
        var titles = new List<string>
        {
            $"Supply Run from {TransportMissionTemplate.VarPickupName} to {TransportMissionTemplate.VarDeliverName}",
            $"Transport of Goods from {TransportMissionTemplate.VarPickupName} to {TransportMissionTemplate.VarDeliverName}",
            $"Delivery to {TransportMissionTemplate.VarDeliverName}",
        };
        
        const string pickupMessage = $"Pickup items at: {TransportMissionTemplate.VarPickupName}";
        const string deliverMessage = $"Deliver items to: {TransportMissionTemplate.VarDeliverName}";

        return new TransportMissionTemplate(
            random.PickOneAtRandom(titles),
            pickupMessage,
            deliverMessage,
            [
                new QuestElementQuantityRef(
                    new ElementId{elementId = (ulong)random.NextInt64(0, long.MaxValue)},
                    new ElementTypeName("FactionSealedContainer"),
                    1
                )
            ]
        );
    }
}