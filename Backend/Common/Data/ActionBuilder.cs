using Newtonsoft.Json;
using NQ;

namespace Mod.DynamicEncounters.Common.Data;

public class ActionBuilder
{
    private readonly ModAction _action = new()
    {
        modName = "Mod.DynamicEncounters",
        playerId = ModBase.Bot.PlayerId,
    };

    public ActionBuilder WithPlayerId(ulong playerId)
    {
        _action.playerId = playerId;
        
        return this;
    }

    public ActionBuilder WithModName(string name)
    {
        _action.modName = name;
        
        return this;
    }
    
    public ActionBuilder WithConstructId(ulong constructId)
    {
        _action.constructId = constructId;
        
        return this;
    }

    public ActionBuilder WithElementId(ulong elementId)
    {
        _action.elementId = elementId;
        
        return this;
    }

    public ActionBuilder WithPayload(object payload)
    {
        _action.payload = JsonConvert.SerializeObject(payload);

        return this;
    }

    public ActionBuilder OpenPlayerBoardApp()
    {
        _action.actionId = 1000004;
        
        return this;
    }
    
    public ActionBuilder GiveTakeContainer()
    {
        _action.actionId = 115;

        return this;
    }

    public ActionBuilder LoadPlayerParty()
    {
        _action.actionId = 103;

        return this;
    }
    
    public ActionBuilder ShootWeapon(object payload)
    {
        _action.actionId = 116;
        
        return WithPayload(payload);
    }

    public ModAction Build() => _action;
}