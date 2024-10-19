using System.Collections.Generic;
using Mod.DynamicEncounters.Features.Loot.Data;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public class TransportMissionTemplate(
    string title,
    string pickupMessage,
    string deliverMessage,
    IEnumerable<ElementQuantityRef> items)
{
    public string Title { get; } = title;
    public string PickupMessage { get; } = pickupMessage;
    public string DeliverMessage { get; } = deliverMessage;
    public IEnumerable<ElementQuantityRef> Items { get; } = items;

    public const string VarPickupName = "@{FROM}";
    public const string VarDeliverName = "@{TO}";

    public TransportMissionTemplate SetPickupConstructName(string constructName)
    {
        return new TransportMissionTemplate(
            Title.Replace(VarPickupName, constructName),
            PickupMessage.Replace(VarPickupName, constructName),
            DeliverMessage.Replace(VarPickupName, constructName),
            Items
        );
    }
    
    public TransportMissionTemplate SetDeliverConstructName(string constructName)
    {
        return new TransportMissionTemplate(
            Title.Replace(VarDeliverName, constructName),
            PickupMessage.Replace(VarDeliverName, constructName),
            DeliverMessage.Replace(VarDeliverName, constructName),
            Items
        );
    }
}