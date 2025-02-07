using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Data;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;

public class ConstructStateService(IServiceProvider provider) : IConstructStateService
{
    private readonly IConstructStateRepository _repository = provider.GetRequiredService<IConstructStateRepository>();
    
    public async Task<ConstructStateOutcome> PersistState(ConstructStateItem stateItem)
    {
        var result = await _repository.Find(stateItem.ConstructId, stateItem.Type);

        if (result == null)
        {
            await _repository.Add(stateItem);
            return ConstructStateOutcome.Added();
        }

        await _repository.Update(stateItem);
        return ConstructStateOutcome.Updated();
    }

    public async Task<ConstructStateOutcome> Find(string type, ulong constructId)
    {
        var result = await _repository.Find(constructId, type);

        if (result == null)
        {
            return ConstructStateOutcome.NotFound(type, constructId);
        }
        
        return ConstructStateOutcome.Retrieved(result);
    }
}