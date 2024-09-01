using System;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors;

public class ErrorHandlerBehavior(IConstructBehavior constructBehavior) : IConstructBehavior
{
    private bool _active = true;

    public bool IsActive() => _active;

    public async Task InitializeAsync(BehaviorContext context)
    {
        if (_active == false)
        {
            return;
        }
        
        try
        {
            await constructBehavior.InitializeAsync(context);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _active = false;
        }
    }

    public async Task TickAsync(BehaviorContext context)
    {
        if (_active == false)
        {
            return;
        }
        
        try
        {
            await constructBehavior.TickAsync(context);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _active = false;
        }
    }
}