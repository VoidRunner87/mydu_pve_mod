using System.Threading.Tasks;
using Mod.DynamicEncounters.Overrides.Common.Data;
using Mod.DynamicEncounters.Overrides.Common.Interfaces;
using Newtonsoft.Json;
using NQ;

namespace Mod.DynamicEncounters.Overrides.Actions;

public class PushConstructDataAction(
    ICachedConstructDataService cachedConstructDataService
) : IModActionHandler
{
    public Task HandleAction(ulong playerId, ModAction action)
    {
        var constructData = JsonConvert.DeserializeObject<ConstructData>(action.payload);
        
        cachedConstructDataService.Set(action.constructId, constructData);

        return Task.CompletedTask;
    }
}