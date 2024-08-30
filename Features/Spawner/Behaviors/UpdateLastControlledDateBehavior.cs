using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors;

public class UpdateLastControlledDateBehavior(ulong constructId) : IConstructBehavior
{
    private bool _active = true;
    private IConstructHandleRepository _repository;

    public bool IsActive() => _active;
    private double _totalDeltaTime = 0;

    public Task InitializeAsync(BehaviorContext context)
    {
        var provider = context.ServiceProvider;
        _repository = provider.GetRequiredService<IConstructHandleRepository>();
        
        return Task.CompletedTask;
    }

    public Task TickAsync(BehaviorContext context)
    {
        if (context.IsAlive || context.IsActiveWreck)
        {
            _totalDeltaTime += context.DeltaTime;
            
            if (_totalDeltaTime > 10)
            {
                _totalDeltaTime = 0;
                return _repository.UpdateLastControlledDateAsync([constructId]);
            }
        }
        else
        {
            _active = false;
        }
        
        return Task.CompletedTask;
    }
}